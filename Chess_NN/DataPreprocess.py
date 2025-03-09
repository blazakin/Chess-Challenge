import chess
import chess.pgn
import chess.engine
import numpy as np
import copy

# 46132506 lines/boards
datafile = r"Chess_NN\data\ficsgamesdb_2012.pgn"


def getxFENS(num_FENS):
    with open(datafile) as data:
        game = chess.pgn.read_game(data)
        result = []
        for i in range(num_FENS):
            result.append(game.board().fen())
            game.next()
    return result


def getxBB(num_FENS):
    with open(datafile) as data:
        game = chess.pgn.read_game(data)
        result = []
        for _ in range(num_FENS):
            result.append(board_to_BB(game.board()))
            game.next()
    return np.asarray(result)


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

    # legal_moves = board.legal_moves
    # legal_board = [[0 for _ in range(8)] for _ in range(8)]
    # for move in legal_moves:
    #     square = move.to_square
    #     row, col = divmod(square, 8)
    #     legal_board[row][col] = 1
    # legal_board = np.asarray(legal_board[::-1]).astype(np.int8)

    w_castling = (np.asarray([board.has_kingside_castling_rights(chess.WHITE),
                              board.has_queenside_castling_rights(chess.WHITE)
                              ])).astype(np.int8)

    b_castling = (np.asarray([board.has_kingside_castling_rights(chess.BLACK),
                              board.has_queenside_castling_rights(chess.BLACK)
                              ])).astype(np.int8)

    # Put pieces to move as normal "white" position
    if board.turn:
        return np.concatenate((np.concatenate((w_pawn, w_rook, w_knight, w_bishop, w_queen, w_king,
                               b_pawn, b_rook, b_knight, b_bishop, b_queen, b_king)).ravel(), w_castling, b_castling))
    else:
        return np.concatenate((np.concatenate((b_pawn[::-1], b_rook[::-1], b_knight[::-1], b_bishop[::-1], b_queen[::-1], b_king[::-1],
                               w_pawn[::-1], w_rook[::-1], w_knight[::-1], w_bishop[::-1], w_queen[::-1], w_king[::-1])).ravel(), b_castling, w_castling))


def _piece_to_np(piece, color, board):
    return np.reshape((np.asarray(board.pieces(
        piece, color).tolist())).astype(np.int8), (8, 8))


def stockfish_evaluation(board, time_limit=0.01):
    engine = chess.engine.SimpleEngine.popen_uci(
        r"Chess_NN\stockfish\stockfish-windows-x86-64-avx2.exe")
    result = engine.analyse(board, chess.engine.Limit(time=time_limit))
    engine.quit()
    return result['score'].relative.score(mate_score=100000)


def BBandEval(num_games, time_per_board=0.01):
    with open(datafile) as data:
        game = chess.pgn.read_game(data)
        engine = chess.engine.SimpleEngine.popen_uci(
            r"Chess_NN\stockfish\stockfish-windows-x86-64-avx2.exe")
        inputs = []
        outputs = []
        for i in range(num_games):
            inputs.append(board_to_BB(copy.deepcopy(game.board())))
            eval = engine.analyse(
                copy.deepcopy(game.board()), chess.engine.Limit(time=time_per_board))

            # Output should already be relative to current player
            outputs.append(eval['score'].relative.score(mate_score=100000))
            game.next()

        inputs = np.asarray(inputs)
        outputs = np.asarray(outputs)
        engine.quit()
    return inputs, outputs


boardstates, evals = BBandEval(1000)
np.save(r"Chess_NN\data\boardstates.npy", boardstates)
np.save(r"Chess_NN\data\evals.npy", boardstates)
