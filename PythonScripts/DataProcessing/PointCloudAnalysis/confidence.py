import numpy as np
from scipy import stats
import numpy as np
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--data', dest="data", type=float, nargs='+', help="Data to measure confidence of")
parser.add_argument('--confidence', dest="confidence", type=float, default=0.95, help="Confidence level")
args = parser.parse_args()



def mean_confidence_interval(data, confidence=0.95):
    """Stolen from stack-overflow"""
    a = np.array(data, dtype=np.float32)
    n = len(a)
    m, se = np.mean(a), stats.sem(a)
    h = se * stats.t.ppf((1 + confidence) / 2., n-1)

    return m, h

if __name__ == "__main__":
    m, h = mean_confidence_interval(args.data, args.confidence)
    print(f"{m} +- {h}")