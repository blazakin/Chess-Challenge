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
        int iterDepth = 3;

        // Iterative deepening to a max depth of 64 and ensure enough time for future moves
        while (iterDepth < 64 && timer.MillisecondsElapsedThisTurn < timer.MillisecondsRemaining / 30) {
            bestMove = Search(-30000, 30000, iterDepth++);
        }
        Console.WriteLine(iterDepth);
        

        return bestMove;

        // Main function to search for the best possible move
        Move Search(int alpha, int beta, int depth) {
            int bestValue = -30000;
            var moves = board.GetLegalMoves(false);
            // Console.WriteLine(moves[0]);
            Move curBestMove = moves[0];

            // Choose best move from available moves
            foreach (Move move in moves) {
                // Needed, but currently messes up search
                // if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 15)
                //     return curBestMove;
                board.MakeMove(move);
                var boardValue = -AlphBet(-beta, -alpha, depth - 1);
                board.UndoMove(move);
                if (boardValue > bestValue) {
                    bestValue = boardValue;
                    curBestMove = move;
                }
                if (boardValue > alpha) {
                    alpha = boardValue;
                }
            }
            return curBestMove;
        }

        int AlphBet(int alpha, int beta, int depth) {
            int bestscore = -30000;
            var moves = board.GetLegalMoves(false);
            if (depth == 0 || timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 15) {
                return quiesce(alpha, beta);
            }
            foreach(Move move in moves) {
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


//         // Evaluation Function --------------------------------------------





//         // Evaluation Feed Forward NN
//         int Evaluate() {
//             float[] x0 = refine();
//             List<float> x = new List<float>(x0);
//             for(int i=0;i < W.Length; i++) {
//                 x = MV_multiply(W[i],x).ToList();
//                 for(int j=0;j < x.Count; j++){
//                     x[j] += B[i][j];
//                 }
//             x = Relu(x);
//             }
//             //int result = (int)(-(float)Math.Log((1/(x[0]))-1)*1000);
//             //return result;
//             return (int)(x[0]*1000);
//         }

//         float[] refine() {
//             float[] magicNum = {1799006.86904004F, 1263811.01656626F, 1471359.55383204F, 1845698.98262769F,
//  1338375.48563436F, 1967142.81661152F,  983261.48628797F,  572963.14041666F,
//   529013.76797665F,  342594.49390919F,  331331.47627541F,  218989.83534127F};
//             float[] totBoard = new float[12];
//             foreach(bool stm in new[] {true, false}) {
//             int num = 0;
//             for(var p = PieceType.Pawn; p <= PieceType.King; p++) {
//                 float mask = board.GetPieceBitboard(p, stm);
//                 if(stm)
//                 totBoard[num ] = mask;
//                 else
//                 totBoard[num + 6] = mask;

//                 num++;
//                 }
//             }
//             for(int iter = 0; iter <=11; iter++) {
//                 totBoard[iter] += 1;
//                 Math.Log(totBoard[iter]);
//                 totBoard[iter] /= magicNum[iter];
//                 totBoard[iter] *= 1000;
//             }
//             return totBoard;
//         }
        
//         List<float> Relu(List<float> X) {
//             for (int i = 0; i < X.Count; i++) {
//                 if (X[i] < 0)
//                     X[i] = 0;
//             }
//             return X;
//         }
//     }

//     float[] MV_multiply(float[,] matrix, List<float> vector) {
//                 float[] U = new float[matrix.GetLength(1)];
//                 for(int i = 0; i < matrix.GetLength(1); i++) {
//                     for(int j = 0; j < matrix.GetLength(0); j++) {
//                         U[i] += matrix[j, i] * vector[j];
//                     }
//                 }
//                 return U;
//             }

//         float[][] B = new float[][] { 
//             new float[] {0.03723057F,-0.02762258F,-0.01822989F,0.01881527F,0.0F,
// 0.01681741F,0.05372256F},

//             new float[] {-0.02900909F,-0.11783109F,0.05222551F,-0.01532215F,0.08988103F},

//             new float[] {0.11955099F}
//         };


//     readonly float[][,] W = new float[][,] { 
//         new float[,] {{-0.2245362F,0.26698455F,-0.06180186F,0.48695573F,-0.2463555F,
// 0.14795984F,-0.7198302F},
// {-0.2327547F,1.4093838F,0.36108053F,1.3187697F,-0.29626F,
// 0.05212346F,-2.403266F},
// {-0.94918567F,1.924019F,-0.51017267F,0.46424535F,-0.0486629F,
// 0.40468052F,-1.2982361F},
// {-0.14670518F,-0.5582919F,0.5218005F,0.36282298F,-0.3485757F,
// 1.1722072F,1.5048163F},
// {0.44167754F,0.2916649F,0.25444445F,0.247489F,-0.33844364F,
// -1.3355412F,-0.64011896F},
// {-0.90532476F,-0.6576307F,-1.077615F,-0.86914754F,-0.24946654F,
// -0.26641902F,0.36790362F},
// {2.3710692F,0.67924076F,2.1382835F,1.3556714F,-0.38678256F,
// 1.4025216F,2.3730643F},
// {0.13229287F,-1.3165954F,0.47243595F,1.2844214F,-0.2680156F,
// 0.07807291F,-0.42814243F},
// {0.7724434F,1.55683F,-0.27742192F,-0.64641577F,-0.28080624F,
// -0.39437747F,0.561956F},
// {0.717485F,-0.12144568F,-0.06022454F,0.32228944F,-0.04870766F,
// -0.15342246F,0.39988837F},
// {0.21623155F,-0.11056086F,0.8012865F,-0.7402706F,-0.11873627F,
// 1.8311926F,0.39109844F},
// {-0.43093172F,-0.30519754F,-0.83965504F,0.2904027F,-0.54140484F,
// 1.3032936F,0.58138674F}},

//             new float[,] {{0.7534533F,0.36069393F,0.01789098F,-0.3618458F,0.8589776F},
// {0.61125696F,-0.36628985F,1.6377976F,0.5661947F,1.0410044F},
// {-0.22709699F,-0.7106891F,0.3642272F,-0.11433335F,1.4423318F},
// {0.7330322F,-0.7759369F,0.846702F,1.3073138F,0.8674749F},
// {0.19072425F,0.55867F,-0.5703055F,-0.4375305F,0.4746259F},
// {-1.6324086F,-0.15931149F,0.54522127F,-0.17149477F,0.1086554F},
// {-0.5892155F,0.30308014F,0.79505754F,-1.1514558F,0.38477027F}},

//             new float[,] {{-1.6760415F},
// {-0.44252953F},
// {0.46493134F},
// {1.2749492F},
// {1.0145607F}}
// }; 




        // Reference evaluate ------------------------

        int Evaluate()
        {
            var accumulators = new int[2, 8];
            int mat = 0, bucket = -2;

            // Adds a feature (colour, piece, square) to given accumulator
            void updateAccumulator(int side, int feature)
            {
                for (int i = 0; i < 8;)
                    accumulators[side, i] += weights[feature * 8 + i++];
            }

            // Initialise with input biases
            updateAccumulator(0, 768);
            updateAccumulator(1, 768);

            for (int stm = 768; (stm -= 384) >= 0;)
            {
                for (var p = 0; p <= 5; p++)
                    for (ulong mask = board.GetPieceBitboard((PieceType)p + 1, stm > 0); mask != 0;)
                    {
                        mat += (int)(0x3847D12C4B064 >> 10 * p & 0x3FF);
                        bucket++;
                        int sq = BitboardHelper.ClearAndGetIndexOfLSB(ref mask);

                        // Add feature from each perspective
                        updateAccumulator(0, 384 - stm + p * 64 + sq);
                        updateAccumulator(1, stm + p * 64 + sq ^ 56);
                    }
                mat = -mat;
            }

            bucket /= 4;

            // Initialise with output bias
            int eval = 8 * raw[1672 + bucket];

            // Compute hidden -> output layer
            for (int i = 0; i < 16;)
                eval += Math.Clamp(accumulators[i / 8 ^ (board.IsWhiteToMove ? 0 : 1), i % 8], 0, 32) * raw[1544 + 16 * bucket + i++];

            // Scale + Material Factoriser
            return eval * 400 / 1024 + (board.IsWhiteToMove ? mat : -mat);
        }
    }

    readonly int[] weights = new int[6152];
    readonly int[] raw = new int[1680];

    public MyBot()
    {
        int i;
        for (i = 0; i < 1680;)
        {
            var packed = decimal.GetBits(packedWeights[i / 16]);
            int num = i % 16 * 6;
            uint adj = (uint)packed[num / 32] >> num % 32 & 63;
            if (num == 30) adj += ((uint)packed[1] & 15) << 2;
            if (num == 60) adj += ((uint)packed[2] & 3) << 4;
            raw[i++] = (int)adj - 31;
        }

        for (i = 0; i < 6144;)
        {
            int sq = i / 8 % 64, pc = i / 512 * 128 + i % 8;
            weights[i++] = raw[pc + sq / 8 * 8] + raw[pc + 64 + sq % 8 * 8];
        }

        for (; i < 6152;)
            weights[i] = raw[i++ - 4608];
    }

    readonly decimal[] packedWeights = {
        37747653452566649643112200159m,40223533531119395795492272480m,36529664745230695484962498788m,38985286316542633163201559024m,
        38985295985106598109212715174m,38946912517662914348042283175m,36491272572183396258467653797m,32835797073131705103024765476m,
        32854243220785540026070853024m,29179712968580801034347984547m,32931911610134636654980605861m,30338489770686330769862403942m,
        34092490140321793794559567652m,30417350850180248481830385445m,30417653007847885573216118309m,30378079431469935389488559781m,
        32912569094329608617403729252m,30456036327645009463511197092m,30455729226267957424288278118m,27921211375468860013618767524m,
        31693674284166331378364175908m,30456041123744425432466171301m,30456343502809024535786526181m,30456031755176046322343319012m,
        26684509342306577591239571435m,25485557233320086643572602861m,25504904695049456221797611501m,25524847101149836653996144685m,
        27942698739172978852429690796m,26782125376487555259915692141m,24247607673262969198062455021m,25445667403639282418961205229m,
        16995979837228230366259918514m,13301502385994229316153059125m,13358925845899911503701943738m,8465797041554692508998518393m,
        15758946271974847145378399478m,15758341881806899673467795127m,14558482859454369201073764150m,16993863922977976472769742390m,
        37766386785038708462021624987m,38927267323141192925855746014m,38985286094209058421285452127m,31557610736976084312536504860m,
        35309547357410518802190149404m,38965962395254748578188482655m,40204195222396982536983414879m,42776463445949534612135737375m,
        47495259823828249531830368223m,41441818714203281256922095647m,41441814137266738764187301600m,38985286316542782417481627551m,
        45116348741624901621440963551m,46258183901752308854824244064m,42525918219260313199796425567m,41443329941643020071853228893m,
        45114841501130143488535566300m,47552338555051126503810431068m,45116041421604578846415407257m,38983765124381909900152408473m,
        42658894895806186162270631902m,46353377073965443271595276250m,46334648094814938202127091929m,41402240046829644290269002202m,
        38944767458615369558953174807m,43877496776303268980199421721m,40125293408959924711446177688m,35250899630839069592040388503m,
        38905784545795402365035497240m,41381967004565682317317269272m,40144631427073220979818649495m,38887343777314141395222419414m,
        52446041348229510066592966229m,50009149419048194965464307350m,50009149491663949555014440532m,47552602559049993130785773205m,
        50027887769251650175637530259m,51208101675590619910330791637m,52427308161960058093334582997m,51305722289067731609430677141m,
        68459529139765064009342462158m,68421136520449335297223624780m,62231436395502995394628348106m,56060483916731290586409781194m,
        58555697291085161888792881100m,64726361925672827280515057740m,65945275403978270817109858443m,64726380523468490112266594316m,
        40223230793709562254239112605m,41441814063605867518515903198m,39004615104276150816811890655m,32834285762283599487748392478m,
        40186373011307798078995175578m,41539434968771821023218235551m,39023665061583105897233508448m,37745547129360348775237154783m,
        18742957259898249745876947m,578060029656081477065113600m,1042084174529075921941626944m,1179301751213326758401520640m,
        4912752746535941460973053830m,12281746060469715462392899535m,19630475849706118524860098581m,30712637518828842268765701974m,
        47768146784521744342789384029m,
    };

    
}