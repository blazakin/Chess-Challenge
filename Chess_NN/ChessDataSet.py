import os
import sys
import math
import numpy as np
from torch.utils.data import Dataset

# DataSet class file for Pytorch


class ChessDataSet(Dataset):
    def __init__(self, root, train=True, transform=None, target_transform=None):
        self.boardstates = np.load(os.path.join(root, "boardstates.npy"))
        self.evals = np.load(os.path.join(root, "evals.npy"))
        self.train = train
        self.transform = transform
        self.target_transform = target_transform
        if self.train:
            self.boardstates = self.boardstates[:math.ceil(
                len(self.boardstates)*.95)]
            self.evals = self.evals[:math.ceil(
                len(self.evals)*.95)]
        else:
            self.boardstates = self.boardstates[:math.floor(
                len(self.boardstates)*.05)]
            self.evals = self.evals[:math.floor(
                len(self.evals)*.05)]

    def __len__(self):
        return len(self.evals)

    def __getitem__(self, idx):
        boardstate = self.boardstates[idx]
        eval = self.evals[idx]
        if self.transform:
            boardstate = self.transform(boardstate)
        if self.target_transform:
            eval = self.target_transform(eval)
        return boardstate, eval
