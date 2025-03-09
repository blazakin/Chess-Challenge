import os
import numpy as np
from torch.utils.data import Dataset

# DataSet class file for Pytorch


class ChessDataSet(Dataset):
    def __init__(self, boardstates_file, evals_file, transform=None, target_transform=None):
        self.boardstates = np.load(boardstates_file)
        self.evals = np.load(evals_file)
        self.transform = transform
        self.target_transform = target_transform

    def __len__(self):
        return len(self.evals)

    def __getitem__(self, idx):
        boardstate = self.boardstates_file[idx]
        eval = self.evals[idx]
        if self.transform:
            boardstate = self.transform(boardstate)
        if self.target_transform:
            eval = self.target_transform(eval)
        return boardstate, eval
