#!/usr/bin/python
# -*- coding: latin-1 -*-import unicodedat

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

def plotDimensionHistorySurface(file_name):
    metrics = util.load_json(util.get_path_to_data_file(file_name))

    connections = metrics["Connections"]
    histories = metrics["Histories"]

    dimensions = range(1, len(histories) + 1)
    steps = range(len(histories[0]["Error"]))

    heights = [[], []]

    for x in range(len(histories)):
        heights[0].append([])
        heights[1].append([])

        for y in steps :
            heights[0][x].append(math.log(histories[x]["Error"][y] / (0.7 * connections)))
            heights[1][x].append(math.log(histories[x]["ValError"][y] / (0.3 * connections)))

    util.plotSurface(heights, "Error e", dimensions, "Dimensions d", steps, "Steps s", 2, ["MSE", "Validation MSE"])

def plotLRStepSurface(file_name, dimension):
    metrics = util.load_json(util.get_path_to_data_file(file_name))

    connections = metrics["Connections"]
    histories = metrics["AllResults"]

    dimension1 = list(histories.values())[0]
    lr_keys = list(dimension1.keys())
    learning_rates = [dimension1[i]["LR"] for i in lr_keys]
    lr_indices = range(len(lr_keys))
    steps = range(len(list(dimension1.values())[0]["Error"]))

    heights = [[], []]

    for x in lr_indices:
        heights[0].append([])
        heights[1].append([])

        for y in steps:
            heights[0][x].append(math.log(histories[dimension][lr_keys[x]]["Error"][y] / (0.7 * connections)))
            heights[1][x].append(math.log(histories[dimension][lr_keys[x]]["ValError"][y] / (0.3 * connections)))

    util.plotSurface(heights, "Error e", learning_rates, "Learning Rate η", 
                steps, "Steps s", 2, ["MSE", "Validation MSE"])

def plotDimensionLRSurface(file_name, only_val):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))

    dimension_keys = list(metrics.keys())
    dimension1 = list(metrics.values())[0]
    lr_keys = dimension1.keys()
    lr_keys = [str(i) for i in range(len(lr_keys))]
    #learning_rates = [float(dimension1[y]["RunHistory"]["LR"]) for y in lr_keys]

    heights = [[], []]
    min_val_error = (math.inf, 0, 0)
    min_dim = min([int(x) for x in dimension_keys])

    for x in dimension_keys:
        heights[0].append([])
        heights[1].append([])
        for y in lr_keys:
            connections = metrics[x][y]["RunHistory"]["ConnectionCount"]
            log_error = math.sqrt(metrics[x][y]["RunHistory"]["Error"][-1] / (0.7 * connections))
            log_val_error = math.sqrt(metrics[x][y]["RunHistory"]["ValError"][-1] / (0.3 * connections))
            if log_val_error > 0.96:
                log_val_error = 0.96
            heights[0][int(x) - min_dim].append(log_val_error)
            if not only_val:
                heights[1][int(x) - min_dim].append(log_error)

            if log_val_error < min_val_error[0]:
                min_val_error = (log_val_error, x, y)
        ...

    print(f"Lowest val error: {min_val_error[0]}, in {min_val_error[1]} dimensions and lr: {min_val_error[2]}")
    util.plotSurface(heights, "Error e", [int(i) for i in dimension_keys], "Dimensions d", 
                [int(x) for x in lr_keys], "Learning Rate η", 2 - int(only_val), ["Val RMS", "RMS"]) #, 

def plotMergeStepLRSurface(file_name):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))

    lr_keys = np.sort(list(metrics.keys()))
    learning_rates = [float(v) for v in lr_keys]
    steps = range(len(metrics[lr_keys[0]]["MergedRun"]["RunHistory"]["Error"]))
    

    heights = [[], []]

    for x in range(len(lr_keys)):
        heights[0].append([])
        heights[1].append([])

        val_split = metrics[lr_keys[x]]["MergedRun"]["ValidationSplit"]
        connection = metrics[lr_keys[x]]["MergedRun"]["RunHistory"]["ConnectionCount"]
       
        for y in steps:
            heights[0][x].append(math.log(metrics[lr_keys[x]]["MergedRun"]["RunHistory"]["Error"][y] / ((1 - val_split) * connection)))
            heights[1][x].append(math.log(metrics[lr_keys[x]]["MergedRun"]["RunHistory"]["ValError"][y] / (val_split * connection)))

    util.plotSurface(heights, "Log Error log(e)", learning_rates, "Learning Rate η", steps, "Step i", 2, ["MSE", "Validation MSE"])

def plotSplitVarySurface(file_name):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))

    ways_strings = np.sort(list(metrics.keys()))
    ways = np.sort([int(v) for v in ways_strings])
    ways_strings = [str(v) for v in ways]
    steps = range(len(metrics[ways_strings[0]]["MergedRun"]["RunHistory"]["Error"]))
    

    heights = [[], []]

    for x in range(len(ways_strings)):
        heights[0].append([])
        heights[1].append([])

        val_split = metrics[ways_strings[x]]["MergedRun"]["ValidationSplit"]
        connection = metrics[ways_strings[x]]["MergedRun"]["RunHistory"]["ConnectionCount"]
       
        for y in steps:
            heights[0][x].append(math.log(metrics[ways_strings[x]]["MergedRun"]["RunHistory"]["Error"][y] / ((1 - val_split) * connection)))
            heights[1][x].append(math.log(metrics[ways_strings[x]]["MergedRun"]["RunHistory"]["ValError"][y] / (val_split * connection)))

    util.plotSurface(heights, "Log Error log(e)", ways, "Ways split", steps, "Step i", 2, ["MSE", "Validation MSE"])

def plotConnectionLRSurface(file_name):
    histories = util.load_json(util.get_path_to_run_history_file(file_name))

    connection_count_keys = list(histories.keys())
    dimension_indices = range(len(connection_count_keys))
    cloud_size_1 = list(histories.values())[0]
    lr_keys = list(cloud_size_1.keys())
    learning_rates = [cloud_size_1[y]["LR"] for y in lr_keys]
    connection_counts = []

    heights = [[], []]

    for x in dimension_indices:
        heights[0].append([])
        heights[1].append([])

        connection_counts.append(histories[connection_count_keys[x]][lr_keys[0]]["ConnectionCount"])

        for y in lr_keys:
            connectionsCount = histories[connection_count_keys[x]][y]["ConnectionCount"]
            heights[0][x].append(math.log(histories[connection_count_keys[x]][y]["Error"][-1] / (0.7 * connectionsCount)))
            heights[1][x].append(math.log(histories[connection_count_keys[x]][y]["ValError"][-1] / (0.3 * connectionsCount)))

    util.plotSurface(heights, "Log Error log(e)", connection_counts, "Connections c", 
                learning_rates, "Learning Rate η", 2, ["MSE", "Validation MSE"])

if __name__ == "__main__":
    #plotDimensionLRSurface("ml_hist_vary_lr_rtd_constant_Adam.json", only_val=True)
    metrics = util.load_json(util.get_path_to_data_file("run_history/ml10m_benchmark_truediff_hist_.json"))
    ...
    #plotSplitVarySurface("movielens_data_points_hist_spl_vary_ways_t1.json")
    #plotConnectionLRSurface("data/movielens_data_points_hist_pc_sizes_all.json")

    #plotLRStepSurface("data/movielens_data_points_hist_all_dims.json", "5")
    #plotDimensionLRSurface("data/movielens_merged.json")
    #mergeData("data/movielens_data_points_hist_all_dims_2.json", "data/movielens_data_points_hist_all_dims.json", "data/movielens_merged.json")