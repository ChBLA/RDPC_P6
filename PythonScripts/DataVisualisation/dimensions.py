import plotting_util as util
import matplotlib.pyplot as plt
import math
import numpy as np
from scipy import stats
from pathlib import Path
import argparse
import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

parser = argparse.ArgumentParser()
parser.add_argument('-q', dest="quiet", action="store_true", help="indicates whether to print only final results")
parser.add_argument('--val_split', dest="val_split", type=float, help="validation split used for accurate plotting")
parser.add_argument('--file', dest='file', type=str, help='name of the file to use')
args = parser.parse_args()

def plotDim(file_name, file_2_name = "", file_3_name = ""):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    metrics_2 = util.load_json(util.get_path_to_run_history_file(file_name[:-5] + "_2.json"))
    metrics_3 = util.load_json(util.get_path_to_run_history_file(file_name[:-5] + "_3.json"))
    metrics |= metrics_2 | metrics_3

    metrics_2 = util.load_json(util.get_path_to_run_history_file(file_2_name))
    dimension_keys_ml = list(metrics_2.keys())

    metrics_3 = util.load_json(util.get_path_to_run_history_file(file_3_name))
    dimension_keys_ml_dit = list(metrics_3.keys())

    dimension_keys = list(metrics.keys())
    lr_keys = list(metrics.values())[0].keys()
    lr_keys = [str(i) for i in range(len(lr_keys))]

    min_dim = int(dimension_keys[0])

    min_val_by_dim_ml_dit = []
    min_val_by_dim_ml = []
    min_val_by_dim = []
    e_by_dim = []

    for x in dimension_keys:
        min_val_by_dim.append(math.inf)
        e_by_dim.append(0)
        for y in lr_keys:
            connections = metrics[x][y]["RunHistory"]["ConnectionCount"]
            log_val_error = math.sqrt(metrics[x][y]["RunHistory"]["ValError"][-1] / (0.3 * connections))
            index = int(x) - min_dim

            if log_val_error < min_val_by_dim[index]:
                min_val_by_dim[index] = log_val_error
                e_by_dim[index] = math.sqrt(metrics[x][y]["RunHistory"]["Error"][-1] / (0.7 * connections))

    plt.plot([int(x) for x in dimension_keys], min_val_by_dim, linestyle='-', marker='o', label="Douban 100 iterations")
    #plt.plot([int(x) for x in dimension_keys], e_by_dim, linestyle='-', marker='o', label="Douban")

    for x in dimension_keys_ml:
        min_val_by_dim_ml.append(math.inf)
        for y in lr_keys:
            connections = metrics_2[x][y]["RunHistory"]["ConnectionCount"]
            log_val_error = math.sqrt(metrics_2[x][y]["RunHistory"]["ValError"][-1] / (0.3 * connections))
            index = int(x) - min_dim

            if log_val_error < min_val_by_dim_ml[index]:
                min_val_by_dim_ml[index] = log_val_error

    plt.plot([int(x) for x in dimension_keys_ml], min_val_by_dim_ml, linestyle='-', marker='o', label="MovieLens 100 iterations")
    best_dit_lr = []

    for x in dimension_keys_ml_dit:
        min_val_by_dim_ml_dit.append(math.inf) #0
        best_dit_lr.append(0)
        index = int(x) - min_dim
        for y in lr_keys:
            connections = metrics_3[x][y]["RunHistory"]["ConnectionCount"]
            log_val_error = math.sqrt(metrics_3[x][y]["RunHistory"]["ValError"][-1] / (0.3 * connections))
            
            #min_val_by_dim_ml_dit[index] += log_val_error


            if log_val_error < min_val_by_dim_ml_dit[index]:
                min_val_by_dim_ml_dit[index] = log_val_error
                best_dit_lr[index] = metrics[x][y]["RunHistory"]["LR"]
        #min_val_by_dim_ml_dit[index] /= len(lr_keys)
        #print(str(min_val_by_dim_ml_dit[index]) + " " + str(index))

    #for i in range(len(min_val_by_dim_ml_dit)):
    #    print(f"lr: {best_dit_lr[i]}, dim: {dimension_keys_ml_dit[i]}")

    #for i in range(1, len(min_val_by_dim_ml_dit)):
    #    min_val_by_dim_ml_dit[i - 1] = (min_val_by_dim_ml_dit[i] + min_val_by_dim_ml_dit[i - 1])/2

    plt.plot([int(x) for x in dimension_keys_ml_dit][2:], min_val_by_dim_ml_dit[2:], linestyle='-', marker='o', label="MovieLens 200 iterations")

    plt.title("RMSE by number of dimensions")
    plt.xlabel("Dimension d")
    plt.ylabel("Validation RMSE")
    plt.legend()

    plt.show()


def plotDim(file_name):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))

    dimension_keys = list(metrics.keys())
    lr_keys = list(metrics.values())[0].keys()
    lr_keys = [str(i) for i in range(len(lr_keys))]

    min_dim = int(dimension_keys[0])

    min_val_by_dim = []
    e_by_dim = []

    for x in dimension_keys:
        min_val_by_dim.append(math.inf)
        e_by_dim.append(0)
        for y in lr_keys:
            connections = metrics[x][y]["RunHistory"]["ConnectionCount"]
            log_val_error = math.sqrt(metrics[x][y]["RunHistory"]["ValError"][-1] / (args.val_split * connections))
            index = int(x) - min_dim

            if log_val_error < min_val_by_dim[index]:
                min_val_by_dim[index] = log_val_error
                e_by_dim[index] = math.sqrt(metrics[x][y]["RunHistory"]["Error"][-1] / ((1.0 - args.val_split) * connections))

    plt.plot([int(x) for x in dimension_keys], min_val_by_dim, linestyle='-', marker='o')

    plt.title("RMSE by number of dimensions")
    plt.xlabel("Dimension d")
    plt.ylabel("Validation RMSE")
    plt.legend()

    plt.show()

if __name__ == "__main__":
    if (not args.file is None):
        plotDim(args.file)
    else:
        plotDim("dn_hist_vary_lr_alldim_Adam.json")
    ...