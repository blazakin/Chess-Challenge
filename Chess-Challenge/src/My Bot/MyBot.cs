﻿// Resources used
// https://github.com/AnshGaikwad/Chess-World/blob/master/play.py
// https://github.com/jw1912/Chess-Challenge/blob/nn/Chess-Challenge/src/My%20Bot/MyBot.cs#L164
// https://web.archive.org/web/20071030084528/http://www.brucemo.com/compchess/programming/alphabeta.htm

using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot {

    public Move Think(Board board, Timer timer) {   
        Move bestMove = default;
        int iterDepth = 1;

        //while (iterDepth < 64 && timer.MillisecondsElapsedThisTurn < timer.MillisecondsRemaining / 30)
            Search(-30000, 30000, iterDepth++);
        

        return bestMove;

        void Search(int alpha, int beta, int depth) {
            int bestValue = -30000;
            var moves = board.GetLegalMoves(false);
            foreach (Move move in moves) {
                board.MakeMove(move);
                var boardValue = -AlphBet(-beta, -alpha, depth - 1);
                if (boardValue > bestValue) {
                    bestValue = boardValue;
                    bestMove = move;
                }
                if (boardValue > alpha) {
                    alpha = boardValue;
                }
                board.UndoMove(move);
            }

            int AlphBet(int alpha, int beta, int depth) {
                int bestscore = -9999;
                var moves1 = board.GetLegalMoves(false);
                if (depth == 0) {
                    return quiesce(alpha, beta);
                }
                foreach(Move move in moves1) {
                    board.MakeMove(move);
                    int val = -AlphBet(-beta, -alpha, depth - 1);
                    board.UndoMove(move);
                    if (val >= beta) {
                        return val;
                    }
                    if (val > bestscore) {
                        bestscore = val;
                    }
                    if (val > alpha) {
                        alpha = val;
                    }
                } 
            return alpha;
            }

            int quiesce(int alpha, int beta) {
                int standPat = Evaluate();
                if (standPat >= beta)
                    return beta;
                if (alpha < standPat)
                    alpha = standPat;
                var moves2 = board.GetLegalMoves(false);
                foreach (Move move in moves2) {
                    if (move.IsCapture) {
                        board.MakeMove(move);
                        int score = -quiesce(-beta, -alpha);
                        board.UndoMove(move);
                        if (score >= beta)
                            return beta;
                        if (score > alpha)
                            alpha = score;
                    }
                }
                return alpha;
            }
        }

        // Evaluation Feed Forward NN
        int Evaluate() {
            float[] x0 = refine();
            List<float> x = new List<float>(x0);
            for(int i=0;i < W.Length; i++) {
                x = MV_multiply(W[i],x).ToList();
                for(int j=0;j < x.Count; j++){
                    x[j] += B[i][j];
                }
            x = Relu(x);
            }
            //int result = (int)(-(float)Math.Log((1/(x[0]))-1)*1000);
            //return result;
            return (int)(x[0]*1000);
        }

        float[] refine() {
            float[] magicNum = {1799006.86904004F, 1263811.01656626F, 1471359.55383204F, 1845698.98262769F,
 1338375.48563436F, 1967142.81661152F,  983261.48628797F,  572963.14041666F,
  529013.76797665F,  342594.49390919F,  331331.47627541F,  218989.83534127F};
            float[] totBoard = new float[12];
            foreach(bool stm in new[] {true, false}) {
            int num = 0;
            for(var p = PieceType.Pawn; p <= PieceType.King; p++) {
                float mask = board.GetPieceBitboard(p, stm);
                if(stm)
                totBoard[num ] = mask;
                else
                totBoard[num + 6] = mask;

                num++;
                }
            }
            for(int iter = 0; iter <=11; iter++) {
                totBoard[iter] += 1;
                Math.Log(totBoard[iter]);
                totBoard[iter] /= magicNum[iter];
                totBoard[iter] *= 1000;
            }
            return totBoard;
        }
        
        List<float> Relu(List<float> X) {
            for (int i = 0; i < X.Count; i++) {
                if (X[i] < 0)
                    X[i] = 0;
            }
            return X;
        }
    }

    float[] MV_multiply(float[,] matrix, List<float> vector) {
                float[] U = new float[matrix.GetLength(1)];
                for(int i = 0; i < matrix.GetLength(1); i++) {
                    for(int j = 0; j < matrix.GetLength(0); j++) {
                        U[i] += matrix[j, i] * vector[j];
                    }
                }
                return U;
            }

        float[][] B = new float[][] { 
            new float[] {0.11723737F,0.04739789F,-0.04721016F,-0.0513659F,-0.01977105F,
-0.0171296F,-0.04078438F,0.06077557F,-0.02213093F,-0.04187755F,
-0.01448254F,-0.04329537F},

            new float[] {-0.04951977F,-0.05373271F,0.05475232F,0.06200641F,-0.0744308F},

            new float[] {-0.01018591F,0.06298393F,0.12146485F,0.07006788F,-0.03254901F},
            
            new float[] {0.14659339F},

        };
    readonly float[][,] W = new float[][,] { 
        new float[,] {{-0.22481346F,0.1312396F,0.268285F,1.0323015F,-0.30938953F,
-0.33087084F,0.03852603F,0.310863F,0.12961313F,-0.15087226F,
-0.04794105F,-0.16185306F},
{-0.43437898F,0.7869183F,0.00329874F,1.9993781F,0.6456894F,
-0.07817657F,-0.6349925F,0.605535F,-1.7682843F,0.4905328F,
0.69328094F,-0.14231059F},
{-0.59942544F,-1.2478712F,0.3267379F,2.402554F,0.4500692F,
0.45790067F,1.1388963F,-0.42114836F,-0.5712486F,-0.3347558F,
0.77317584F,0.25370178F},
{-1.2444826F,0.83620745F,0.21237402F,-0.7634361F,0.7479882F,
-0.01680825F,0.02283893F,1.0830375F,-0.3547379F,-0.57823414F,
1.1620485F,-0.2147775F},
{0.59395254F,-1.5120497F,-0.21597579F,0.86863154F,-0.6459297F,
0.02674129F,-0.66550666F,0.6602001F,1.7335107F,0.39380112F,
-0.4724946F,0.38193917F},
{0.45921892F,-0.6301756F,-0.24101101F,-0.6688333F,0.02634391F,
-0.1117788F,0.15068077F,-1.2114384F,-1.8509108F,-1.0077794F,
0.4952792F,0.3203923F},
{-1.3975203F,1.0765638F,0.3706178F,-0.34184235F,0.696712F,
-0.2727219F,0.79090416F,0.448466F,2.1476042F,2.584801F,
1.4587469F,0.33962607F},
{0.03803999F,0.14590356F,-0.26803893F,-0.24985413F,0.4286213F,
0.20776351F,-0.9050949F,-0.29927027F,-0.11281504F,1.106796F,
0.13274595F,0.4246461F},
{0.43146846F,0.4602753F,-0.18915892F,0.36034498F,-0.0112653F,
0.20008074F,0.62207174F,0.17818312F,0.84032124F,0.01909764F,
-0.14190824F,0.23872449F},
{0.44813222F,-0.4307819F,-0.36538723F,-0.384827F,0.31634098F,
-0.23900211F,0.93335855F,0.53627175F,-0.06096051F,0.13759331F,
0.11288443F,-0.48860848F},
{-0.49415702F,-0.8280184F,0.27142355F,-0.47058818F,1.7882607F,
-0.27459162F,-0.25492463F,0.6079527F,0.23687384F,0.01245972F,
1.658062F,-0.03628035F},
{-0.4220273F,-0.34602502F,-0.01011898F,-0.38460302F,0.4773567F,
-0.04662962F,0.58069867F,-1.8288144F,-0.49495775F,0.46294615F,
0.43018788F,-0.36221582F}},

            new float[,] {{0.22885773F,-0.02711163F,-0.10427574F,-0.7757643F,0.49875468F},
{-0.24985288F,0.4647219F,0.97788465F,0.43488201F,0.5924289F},
{-0.14836903F,-0.44857407F,0.02046444F,-0.37814957F,0.21148188F},
{-0.2586678F,0.38946056F,1.9189144F,1.980718F,1.1891335F},
{0.04255979F,0.36217904F,0.21411733F,-0.12692949F,-0.7378027F},
{-0.27520847F,0.5373808F,-0.22513354F,-0.18339339F,-0.5638635F},
{-0.4692731F,-0.42555487F,0.7823027F,1.3262957F,0.03921712F},
{0.28503382F,-0.09942792F,0.35936552F,0.90801877F,0.7112048F},
{0.15895759F,-0.09872803F,1.6509297F,1.7380673F,0.9994445F},
{-0.4639481F,-0.57798237F,1.4835035F,1.8734177F,1.1709979F},
{-0.46041754F,-0.4157908F,0.38021886F,-0.00784224F,-0.989219F},
{0.258127F,-0.3917007F,-0.48649743F,0.2580305F,-0.42943433F}},

            new float[,] {{0.47152263F,-0.5019178F,0.1990693F,0.04323915F,-0.7614701F},
{0.71614575F,-0.09410569F,-0.6547231F,-0.62308246F,-0.17379811F},
{-0.60665953F,-0.2053452F,0.533134F,0.760377F,0.01212649F},
{0.0955653F,-1.029651F,0.65836734F,0.05285278F,-0.23489808F},
{0.1792407F,1.5341669F,-1.3252302F,-1.9816189F,0.46055174F}},

            new float[,] {{-0.31156135F},
{1.8345088F},
{1.1355478F},
{0.33168754F},
{-0.22971936F}}

}; 

    
}