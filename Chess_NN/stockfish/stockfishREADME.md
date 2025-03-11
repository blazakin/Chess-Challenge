# Stockfish Folder Readme

Include stockfish or other evaluation function to train off of here.
Change the line 

```Python
engine = chess.engine.SimpleEngine.popen_uci(
            r"path\to\evaluation.exe")
```

in ```BBandEval()``` in DataPreprocess.py