import os
import chess
import chess.pgn
import chess.engine
import numpy as np
from npy_append_array import NpyAppendArray

# Data sourced from https://www.ficsgames.org/download.html
# Used "Standard (all ratings)" for all year 2012, not including move times
# 46132506 lines/boards
datafile = r"Chess_NN\data\ficsgamesdb_2012.pgn"


def board_to_BB(board):
    w_pawn = _piece_to_np(chess.PAWN, chess.WHITE, board)
    w_rook = _piece_to_np(chess.ROOK, chess.WHITE, board)
    w_knight = _piece_to_np(chess.KNIGHT, chess.WHITE, board)
    w_bishop = _piece_to_np(chess.BISHOP, chess.WHITE, board)
    w_queen = _piece_to_np(chess.QUEEN, chess.WHITE, board)
    w_king = _piece_to_np(chess.KING, chess.WHITE, board)

    b_pawn = _piece_to_np(chess.PAWN, chess.BLACK, board)
    b_rook = _piece_to_np(chess.ROOK, chess.BLACK, board)
    b_knight = _piece_to_np(chess.KNIGHT, chess.BLACK, board)
    b_bishop = _piece_to_np(chess.BISHOP, chess.BLACK, board)
    b_queen = _piece_to_np(chess.QUEEN, chess.BLACK, board)
    b_king = _piece_to_np(chess.KING, chess.BLACK, board)
    # print(w_king)

    w_castling = (np.asarray([board.has_kingside_castling_rights(chess.WHITE),
                              board.has_queenside_castling_rights(chess.WHITE)
                              ])).astype(np.int8)

    b_castling = (np.asarray([board.has_kingside_castling_rights(chess.BLACK),
                              board.has_queenside_castling_rights(chess.BLACK)
                              ])).astype(np.int8)

    # Put pieces with repsect to player to move as normal "white" position
    if board.turn:
        return np.concatenate((np.concatenate((w_pawn, w_rook, w_knight, w_bishop, w_queen, w_king,
                               b_pawn, b_rook, b_knight, b_bishop, b_queen, b_king)).ravel(), w_castling, b_castling))
    else:
        return np.concatenate((np.concatenate((b_pawn[::-1], b_rook[::-1], b_knight[::-1], b_bishop[::-1], b_queen[::-1], b_king[::-1],
                               w_pawn[::-1], w_rook[::-1], w_knight[::-1], w_bishop[::-1], w_queen[::-1], w_king[::-1])).ravel(), b_castling, w_castling))


# Converts chess piece board to np array with 1s where the piece type is and 0s everywhere else
def _piece_to_np(piece, color, board):
    return np.reshape((np.asarray(board.pieces(piece, color).tolist())).astype(np.int8), (8, 8))[::-1]


def board_to_BB2(board):
    black, white = board.occupied_co
    w_bitboards = np.array([
        white & board.pawns,
        white & board.rooks,
        white & board.knights,
        white & board.bishops,
        white & board.queens,
        white & board.kings
    ], dtype=np.uint64)

    b_bitboards = np.array([
        black & board.pawns,
        black & board.rooks,
        black & board.knights,
        black & board.bishops,
        black & board.queens,
        black & board.kings
    ], dtype=np.uint64)

    w_castling = np.array([
        board.has_kingside_castling_rights(chess.WHITE),
        board.has_queenside_castling_rights(chess.WHITE)
    ], dtype=np.int8)

    b_castling = np.array([
        board.has_kingside_castling_rights(chess.BLACK),
        board.has_queenside_castling_rights(chess.BLACK)
    ], dtype=np.int8)

    w_board_array = bitboards_to_array(w_bitboards)
    b_board_array = bitboards_to_array(b_bitboards)

    if board.turn:
        return np.concatenate((w_board_array.ravel(), b_board_array.ravel(), w_castling, b_castling))
    else:
        return np.concatenate(([pieces[::-1] for pieces in b_board_array].ravel(),
                               [pieces[::-1]
                                   for pieces in w_board_array].ravel(),
                               b_castling,
                               w_castling))


def bitboards_to_array(bb):
    bb = np.asarray(bb, dtype=np.uint64)[:, np.newaxis]
    s = 8 * np.arange(7, -1, -1, dtype=np.uint64)
    b = (bb >> s).astype(np.uint8)
    b = np.unpackbits(b, bitorder="little")
    return b.reshape(-1, 8, 8).astype(np.int8)


# Example of stockfish evaluations of board state
def stockfish_evaluation(board, time_limit=0.01):
    engine = chess.engine.SimpleEngine.popen_uci(
        r"Chess_NN\stockfish\stockfish-windows-x86-64-avx2.exe")
    result = engine.analyse(board, chess.engine.Limit(time=time_limit))
    engine.quit()
    return result['score'].relative.score(mate_score=100000)


# Start to end-1 inclusive for indexing 0,1,2,...
def BBandEval(*, start=0, end, data_dir, append, time_per_board=0.01):
    # Create valid file paths for processed data files
    boardstates_file = os.path.join(data_dir, "boardstates.npy")
    evals_file = os.path.join(data_dir, "evals.npy")

    # Open pgn datafile
    with open(datafile) as data:
        game = chess.pgn.read_game(data)

        # Get to correct starting point in pgn
        for _ in range(start):
            game.next()

        # Start up evaluation to train to
        engine = chess.engine.SimpleEngine.popen_uci(
            r"Chess_NN\stockfish\stockfish-windows-x86-64-avx2.exe")

        with (NpyAppendArray(boardstates_file, delete_if_exists=not append) as boardstates_npy,
              NpyAppendArray(evals_file, delete_if_exists=not append) as evals_npy):
            for i in range(end-start):
                # saved as int8s, need to be converted to float 32 when read
                boardstates_npy.append(np.asarray([board_to_BB2(
                    game.board())]))
                eval = engine.analyse(
                    game.board(), chess.engine.Limit(time=time_per_board))

                # Output should already be relative to current player
                evals_npy.append(np.asarray([[eval['score'].relative.score(
                    mate_score=100)/100]]).astype(np.float32))

                if i % ((start-end)/10) == 0:
                    pass
                    print(i)
                game.next()
        engine.quit()


BBandEval(
    start=0,
    end=1000,
    data_dir=r"Chess_NN\data\DataSet",
    append=False
)
