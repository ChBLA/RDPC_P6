#!/usr/bin/python
# -*- coding: latin-1 -*-import unicodedat

from cmath import inf
import plotting_util as util
import matplotlib.pyplot as plt
import matplotlib as mpl
import numpy as np;
import matplotlib.cm as cm
import math

from pathlib import Path

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()


def plotDualLR(file_name):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))

    lr1Keys = list(metrics.keys())
    lr2Keys = list(metrics[lr1Keys[0]].keys())

    lr1 = [metrics[i]["0"]["DPMergeLR"][0] for i in lr1Keys]
    lr2 = [metrics["0"][i]["DPMergeLR"][1] for i in lr2Keys]

    heights = [[], []]

    for x in range(len(lr1Keys)):
        heights[0].append([])
        heights[1].append([])

        for y in lr2Keys:
            val_split = metrics[str(x)][y]["MergedRun"]["ValidationSplit"]
            connections = metrics[str(x)][y]["MergedRun"]["RunHistory"]["ConnectionCount"]
            heights[0][x].append(math.log(metrics[str(x)][y]["MergedRun"]["RunHistory"]["Error"][0] / ((1 - val_split) * connections)))
            heights[1][x].append(math.log(metrics[str(x)][y]["MergedRun"]["RunHistory"]["ValError"][0] / (val_split * connections)))

    util.plotSurface(heights, "ln Error ln(e)", lr1, "Learning Rate 1 lr1", 
                     lr2, "Learning Rate 2 lr2", 2, ["MSE", "Validation MSE"])

def getHeightsData(metrics):
    lr1Keys = list(metrics.keys())
    lr2Keys = list(metrics[lr1Keys[0]].keys())

    heights = [[], []]

    for x in range(len(lr1Keys)):
        heights[0].append([])
        heights[1].append([])

        for y in lr2Keys:
            val_split = metrics[str(x)][y]["MergedRun"]["ValidationSplit"]
            connections = metrics[str(x)][y]["MergedRun"]["RunHistory"]["ConnectionCount"]
            heights[0][x].append(math.log(metrics[str(x)][y]["MergedRun"]["RunHistory"]["Error"][0] / ((1 - val_split) * connections)))
            heights[1][x].append(math.log(metrics[str(x)][y]["MergedRun"]["RunHistory"]["ValError"][0] / (val_split * connections)))
    return heights

# Requires that quadrants are given as [1, 2, 3, 4]
def plotDualLRCombined(data):
    metrics = []
    for q in data:
        metrics.append(util.load_json(util.get_path_to_run_history_file(q)))

    # Determine quadrants: (total of 4 quadrants)
    quadrants = [[0, 0], [0, 0]]
    for q in range(len(metrics)):
        quadrants[(q + q // 2) % 2][q // 2] = metrics[q]

    # Generate learning rates:
    lr1 = [quadrants[0][0][str(i)]["0"]["DPMergeLR"][0] for i in range(len(quadrants[0][0]) - 1)] + [quadrants[1][0][str(i)]["0"]["DPMergeLR"][0] for i in range(len(quadrants[1][0]))]
    lr2 = [quadrants[0][0]["0"][str(i)]["DPMergeLR"][1] for i in range(len(quadrants[0][0]["0"]) - 1)] + [quadrants[0][1]["0"][str(i)]["DPMergeLR"][1] for i in range(len(quadrants[0][1]["0"]))]

    # Insert data into heights array
    heights = [[], []]
    min_verror = inf
    min_verror_index = (-1, -1)
    for x in range(len(lr1)):
        heights[0].append([])
        heights[1].append([])
        len_x = (len(quadrants[0][0]) - 1)
        x_i = 1 if x >= len_x else 0 

        for y in range(len(lr2)):
            len_y = (len(quadrants[0][0]["0"]) - 1)
            y_i = 1 if y >= len_y else 0
            metric = quadrants[x_i][y_i] 
            acc_x = str(x if x_i == 0 else x - len_x)
            acc_y = str(y if y_i == 0 else y - len_y)
            val_split = metric[acc_x][acc_y]["MergedRun"]["ValidationSplit"]
            connections = metric[acc_x][acc_y]["MergedRun"]["RunHistory"]["ConnectionCount"]
            curr_verror = math.log(metric[acc_x][acc_y]["MergedRun"]["RunHistory"]["ValError"][0] / (val_split * connections))
            min_verror = min(min_verror, curr_verror)
            min_verror_index = min_verror_index if curr_verror > min_verror else (x, y)
            heights[0][x].append(math.log(metric[acc_x][acc_y]["MergedRun"]["RunHistory"]["Error"][0] / ((1 - val_split) * connections)))
            heights[1][x].append(curr_verror)
    print(f"Min error: {min_verror},  found at: {min_verror_index},  lr: ({lr1[min_verror_index[0]]}, {lr2[min_verror_index[1]]})")
    util.plotSurface(heights, "ln Error ln(e)", lr1, "Learning Rate 1 lr1", 
                     lr2, "Learning Rate 2 lr2", 2, ["MSE", "Validation MSE"])

if __name__ == "__main__":
    plotDualLR("ml_diff1_lr_lrdecay_tuning_v2_hist_.json")
    #plotDualLRCombined(["ml_hist_dual_lr_diff_run_q3_t1.json", "ml_hist_dual_lr_diff_run_q4_t1.json", "ml_hist_dual_lr_diff_run_q1_t1.json", "ml_hist_dual_lr_diff_run_q2_t1.json"])
    ...