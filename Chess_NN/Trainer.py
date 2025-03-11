import torch
from torch import nn
from torch.utils.data import DataLoader
import matplotlib.pyplot as plt
import ChessDataSet

import numpy as np

device = torch.accelerator.current_accelerator(
).type if torch.accelerator.is_available() else "cpu"


class NeuralNetwork(nn.Module):
    def __init__(self):
        super().__init__()
        # self.flatten = nn.Flatten()
        self.linear_relu_stack = nn.Sequential(
            # nn.Linear((8*8*12)+4, 1024),
            # nn.ReLU(),
            # nn.Linear(1024, 2048),
            # nn.ReLU(),
            # nn.Linear(2048, 1024),
            # nn.ReLU(),
            # nn.Linear(1024, 512),
            # nn.ReLU(),
            # nn.Linear(512, 1),
            # nn.Sigmoid()
            nn.Linear((8*8*12)+4, 16),
            nn.ReLU(),
            nn.Linear(16, 64),
            nn.ReLU(),
            nn.Linear(64, 16),
            nn.ReLU(),
            nn.Linear(16, 1),
        )

    def forward(self, x):
        # x = self.flatten(x)
        logits = self.linear_relu_stack(x)
        return logits


def train_loop(dataloader, model, loss_fn, optimizer):
    size = len(dataloader.dataset)
    # Set the model to training mode - important for batch normalization and dropout layers
    # Unnecessary in this situation but added for best practices
    model.train()
    for batch, (X, y) in enumerate(dataloader):
        # Compute prediction and loss
        pred = model(X)
        loss = loss_fn(pred, y)

        # Backpropagation
        loss.backward()
        optimizer.step()
        optimizer.zero_grad()

        if batch % (size/(5*len(X))) == 0:
            loss, current = loss.item(), batch * batch_size + len(X)
            print(f"loss: {loss:>7f}  [{current:>5d}/{size:>5d}]")


def test_loop(dataloader, model, loss_fn):
    # Set the model to evaluation mode - important for batch normalization and dropout layers
    # Unnecessary in this situation but added for best practices
    model.eval()
    size = len(dataloader.dataset)
    num_batches = len(dataloader)
    N = 10
    test_loss, correct, incorrect = 0, 0, 0
    buckets = np.zeros(N)

    # Evaluating the model with torch.no_grad() ensures that no gradients are computed during test mode
    # also serves to reduce unnecessary gradient computations and memory usage for tensors with requires_grad=True
    with torch.no_grad():
        for X, y in dataloader:
            pred = model(X)
            test_loss += loss_fn(pred, y).item()
            correct += (pred - y < .01).type(torch.float).sum().item()
            incorrect += (pred - y > .02).type(torch.float).sum().item()
            for i in range(N):
                buckets[i] += ((abs(pred - y) < .01*i)
                               ).type(torch.float).sum().item()

    test_loss /= num_batches
    correct /= size
    incorrect /= size
    buckets /= size
    # plt.plot(buckets)

    # plt.show()
    print(
        f"Test Error: \n Accuracy: {(100*correct):>0.1f}%, Inaccuracy: {(100*incorrect):>0.1f}%, Avg loss: {test_loss:>8f} \n")


training_data = ChessDataSet.ChessDataSet(
    root=r".\Chess_NN\data\DataSet",
    train=True,
    transform=torch.from_numpy,
    target_transform=torch.from_numpy
)

test_data = ChessDataSet.ChessDataSet(
    root=r".\Chess_NN\data\DataSet",
    train=False,
    transform=torch.from_numpy,
    target_transform=torch.from_numpy
)

model = NeuralNetwork()
learning_rate = 1e-4
batch_size = 100
epochs = 50

loss_fn = nn.MSELoss()
optimizer = torch.optim.AdamW(model.parameters(), lr=learning_rate)

train_dataloader = DataLoader(training_data, batch_size, shuffle=True)
test_dataloader = DataLoader(test_data, batch_size, shuffle=True)


for t in range(epochs):
    print(f"Epoch {t+1}\n-------------------------------")
    train_loop(train_dataloader, model, loss_fn, optimizer)
    test_loop(test_dataloader, model, loss_fn)
print("Done!")


torch.save(model.state_dict(), r"Chess_NN\weights\model_weights.pth")
