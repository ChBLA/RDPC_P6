from ast import arg
import plotting_util as util
import matplotlib.pyplot as plt
import math
import numpy as np
from scipy import stats
from pathlib import Path

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('-q', dest="quiet", action="store_true", help="indicates whether to print only final results")
parser.add_argument('--func', dest="func", type=str, default="constant", help="using multiple files and measure confidence")
parser.add_argument('--file', dest='file', type=str, help='name of the file to use')
parser.add_argument('--constant', dest="constant", type=float, default=6.4, help="constant term for rtd function")
parser.add_argument('--c_init', dest="constant_init", type=float, default=6.0, help="initial constant value")
parser.add_argument('--c_step', dest="constant_step", type=float, default=0.2, help="change per step for constant term")
parser.add_argument('--power', dest="power", type=float, default=1.5, help="power term for rtd function")
parser.add_argument('--p_init', dest="power_init", type=float, default=0.5, help="initial power value")
parser.add_argument('--p_step', dest="power_step", type=float, default=0.1, help="change per step for power term")
parser.add_argument('--val_split', dest="val_split", type=float, help="validation split used for accurate plotting")
args = parser.parse_args()

def plotMinRTDConstantCombined(file_name, file_name_2, only_val):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    metrics_2 = util.load_json(util.get_path_to_run_history_file(file_name_2))

    lr_keys = list(metrics.keys())
    power_keys = list(metrics.values())[0].keys()
    power_keys = [str(i) for i in range(len(power_keys))]

    power_keys_2 = list(metrics_2.values())[0].keys()
    power_keys_2 = [str(i) for i in range(len(power_keys_2))]
    #learning_rates = [float(dimension1[y]["RunHistory"]["LR"]) for y in lr_keys]

    heights_2 = [[], []]
    heights = [[], []]
    min_val_error = (math.inf, 0, 0)
    min_val_error_2 = (math.inf, 0, 0)

    for x in lr_keys:
        heights[0].append([])
        heights[1].append([])
        for y in power_keys:
            connections = metrics[x][y]["RunHistory"]["ConnectionCount"]
            log_error = math.sqrt(metrics[x][y]["RunHistory"]["Error"][-1] / (0.7 * connections))
            log_val_error = math.sqrt(metrics[x][y]["RunHistory"]["ValError"][-1] / (0.3 * connections))
            if log_val_error > 0.96:
                log_val_error = 0.96
            heights[0][int(x)].append(log_val_error)
            if not only_val:
                heights[1][int(x)].append(log_error)

            if log_val_error < min_val_error[0]:
                min_val_error = (log_val_error, x, y)
        ...
    
    for x in lr_keys:
        heights_2[0].append([])
        heights_2[1].append([])
        for y in power_keys_2:
            connections = metrics_2[x][y]["RunHistory"]["ConnectionCount"]
            log_error = math.sqrt(metrics_2[x][y]["RunHistory"]["Error"][-1] / (0.7 * connections))
            log_val_error = math.sqrt(metrics_2[x][y]["RunHistory"]["ValError"][-1] / (0.3 * connections))
            if log_val_error > 0.96:
                log_val_error = 0.96
            heights_2[0][int(x)].append(log_val_error)
            if not only_val:
                heights_2[1][int(x)].append(log_error)

            if log_val_error < min_val_error_2[0]:
                min_val_error_2 = (log_val_error, x, y)
        ...

    min_vals = [min([heights[0][ii][i] for ii in range(len(lr_keys))]) for i in range(len(power_keys))]
    plt.plot([int(p) * 0.2 + 5 for p in power_keys], min_vals, linestyle='-', marker='o', label="Douban")
    
    min_vals = [min([heights_2[0][ii][i] for ii in range(len(lr_keys))]) for i in range(len(power_keys_2))]
    plt.plot([int(p) * 0.2 + 5 for p in power_keys_2], min_vals, linestyle='-', marker='o', label="MovieLens")

    plt.title("RMSE by rtd constant")
    plt.xlabel("rtd constant q")
    plt.ylabel("Validation RMSE")
    plt.legend()

    plt.show()

def plotRTDPower(file_name, only_val = True):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))

    lr_keys = list(metrics.keys())
    power_keys = list(metrics.values())[0].keys()
    power_keys = [str(i) for i in range(len(power_keys))]

    heights = [[], []]
    min_val_error = (math.inf, 0, 0)

    for x in lr_keys:
        heights[0].append([])
        heights[1].append([])
        for y in power_keys:
            connections = metrics[x][y]["RunHistory"]["ConnectionCount"]
            log_error = math.sqrt(metrics[x][y]["RunHistory"]["Error"][-1] / ((1.0 - args.val_split) * connections))
            log_val_error = math.sqrt(metrics[x][y]["RunHistory"]["ValError"][-1] / (args.val_split * connections))
            heights[0][int(x)].append(log_val_error)
            if not only_val:
                heights[1][int(x)].append(log_error)

            if log_val_error < min_val_error[0]:
                min_val_error = (log_val_error, x, y)
        ...

    min_vals = [min([heights[0][ii][i] for ii in range(len(lr_keys))]) for i in range(len(power_keys))]
    plot_keys = [int(p) * args.power_step + args.power_init for p in power_keys]
    plt.plot(plot_keys, min_vals, linestyle='-', marker='o')

    plt.title("RMSE by rtd power")
    plt.xlabel("rtd power p")
    plt.ylabel("Validation RMSE")

    plt.show()

def plotRTDConstant(file_name, only_val = True):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))

    lr_keys = list(metrics.keys())
    power_keys = list(metrics.values())[0].keys()
    power_keys = [str(i) for i in range(len(power_keys))]

    heights = [[], []]
    min_val_error = (math.inf, 0, 0)

    for x in lr_keys:
        heights[0].append([])
        heights[1].append([])
        for y in power_keys:
            connections = metrics[x][y]["RunHistory"]["ConnectionCount"]
            log_error = math.sqrt(metrics[x][y]["RunHistory"]["Error"][-1] / ((1.0 - args.val_split) * connections))
            log_val_error = math.sqrt(metrics[x][y]["RunHistory"]["ValError"][-1] / (args.val_split * connections))
            heights[0][int(x)].append(log_val_error)
            if not only_val:
                heights[1][int(x)].append(log_error)

            if log_val_error < min_val_error[0]:
                min_val_error = (log_val_error, x, y)
        ...

    min_vals = [min([heights[0][ii][i] for ii in range(len(lr_keys))]) for i in range(len(power_keys))]
    plot_keys = [int(p) * args.constant_step + args.constant_init for p in power_keys]
    plt.plot(plot_keys, min_vals, linestyle='-', marker='o')

    plt.title("RMSE by rtd constant")
    plt.xlabel("rtd constant q")
    plt.ylabel("Validation RMSE")

    plt.show()

if __name__ == "__main__":
    if (not args.file is None):
        if (args.func == "constant"):
            plotRTDConstant(args.file)
        elif (args.func == "power"):
            plotRTDPower(args.file)
        else:
            print("No valid function parameter supplied")
    else:
        plotRTDPower("dn_hist_vary_lr_rtd_constant_Adam.json")
        plotRTDConstant("dn_hist_vary_lr_rtd_constant_Adam.json")