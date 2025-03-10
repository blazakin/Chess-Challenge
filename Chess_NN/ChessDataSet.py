import os
import math
import numpy as np
from torch.utils.data import Dataset

# DataSet class file for Pytorch


class ChessDataSet(Dataset):
    def __init__(self, root, train=True, transform=None, target_transform=None):
        # file paths for boardstates and evals
        self.boardstates_file = os.path.join(root, "boardstates.npy")
        self.evals_file = os.path.join(root, "evals.npy")

        # Load files in memory mapped mode (to convserve RAM)
        # 'c’ Copy-on-write: assignments affect data in memory,
        # but changes are not saved to disk. The file on disk is read-only.
        self.evals = np.load(self.evals_file, mmap_mode='c')
        self.boardstates = np.load(self.boardstates_file, mmap_mode='c')

        self.train = train
        self.transform = transform
        self.target_transform = target_transform
        # if self.train:
        #     self.boardstates = self.boardstates[:math.floor(
        #         len(self.boardstates)*.95)]
        #     self.evals = self.evals[:math.floor(
        #         len(self.evals)*.95)]
        # else:
        #     self.boardstates = self.boardstates[:math.ceil(
        #         len(self.boardstates)*.05)]
        #     self.evals = self.evals[:math.ceil(
        #         len(self.evals)*.05)]

    def __len__(self):
        if self.train:
            return math.floor(self.evals.shape[0]*.95)
        else:
            return math.ceil(self.evals.shape[0]*.05)

    def __getitem__(self, idx):
        # Adjusts index for testing range
        if not self.train:
            idx = idx + math.floor(self.evals.shape[0]*.95)

        # Ensures training objects cannot access testing data
        elif idx > math.floor(self.evals.shape[0]*.95) - 1:
            raise IndexError("list index out of range")

        boardstate = self.boardstates[idx].astype(np.float32)
        eval = self.evals[idx]
        if self.transform:
            boardstate = self.transform(boardstate)
        if self.target_transform:
            eval = self.target_transform(eval)
        return boardstate, eval
