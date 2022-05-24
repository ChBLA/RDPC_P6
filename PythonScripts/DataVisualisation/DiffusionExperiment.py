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


def plot2DErrorOverTime(file_name, machine):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    steps = [range(len(metrics["DiffusionSteps"][str(i)]["SubcloudRuns"]["0"]["RunHistory"]["Error"])) for i in range(len(metrics["DiffusionSteps"]))]
    runs = [metrics["DiffusionSteps"][str(i)]["SubcloudRuns"][str(machine)] for i in range(len(metrics["DiffusionSteps"]))]
    run_names = [f"M{machine}_Step{i}" for i in range(len(metrics["DiffusionSteps"]))]
    runs_processed = []

    for run in runs:
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        runs_processed.append([math.log(err / ((1 - val_split) * conn_count)) for err in run["RunHistory"]["Error"]])

    for i in range(len(runs_processed)):
        plt.plot(steps[i], runs_processed[i], linestyle='-', marker='o', label=run_names[i])
    
    plt.legend()    
    plt.show()

def plot2DValErrorOverTime(file_name, machine):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    steps = [range(len(metrics["DiffusionSteps"][str(i)]["SubcloudRuns"]["0"]["RunHistory"]["ValError"])) for i in range(len(metrics["DiffusionSteps"]))]
    runs = [metrics["DiffusionSteps"][str(i)]["SubcloudRuns"][str(machine)] for i in range(len(metrics["DiffusionSteps"]))]
    run_names = [f"M{machine}_Step{i}" for i in range(len(metrics["DiffusionSteps"]))]
    runs_processed = []

    for run in runs:
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        runs_processed.append([math.log(err / (val_split * conn_count)) for err in run["RunHistory"]["ValError"]])

    for i in range(len(runs_processed)):
        plt.plot(steps[i], runs_processed[i], linestyle='-', marker='o', label=run_names[i])
    
    plt.legend()
    plt.show()

def Plot2DDiffValOriginalVSMerged(file_name):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    steps = range(len(metrics["OriginalRun"]["RunHistory"]["ValError"]))

    run_names = ["OriginalRun", "MergedRun"]
    runs = [metrics["OriginalRun"], metrics["MergedRun"]]
    runs_processed = []

    for run in runs:
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        runs_processed.append([math.log(err / (val_split * conn_count)) for err in run["RunHistory"]["ValError"]])

    for i in range(len(runs_processed)):
        plt.plot(steps, runs_processed[i], linestyle='-', marker='o', label=run_names[i])
    
    plt.legend()
    plt.show()

def Plot2DDiffOriginalVSMerged(file_name):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    steps = range(len(metrics["OriginalRun"]["RunHistory"]["Error"]))

    run_names = ["OriginalRun", "MergedRun"]
    runs = [metrics["OriginalRun"], metrics["MergedRun"]]
    runs_processed = []

    for run in runs:
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        runs_processed.append([math.log(err / ((1-val_split) * conn_count)) for err in run["RunHistory"]["Error"]])

    for i in range(len(runs_processed)):
        plt.plot(steps, runs_processed[i], linestyle='-', marker='o', label=run_names[i])
    
    plt.legend()
    plt.show()

def Plot2DMergedErrorDiff(file_name):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    steps = range(len(metrics["DiffusionSteps"]))

    run = [metrics["DiffusionSteps"][str(i)]["MergedTotalError"] for i in steps]
    run = [metrics["OriginalRun"]["RunHistory"]["Error"][0]/2] + run
    run_processed = [math.log(err / metrics["ConnectionsAfterMerge"]) for err in run]

    
    plt.plot(range(len(run)), run_processed, linestyle='-', marker='o', label="Merge Error")
    
    plt.legend()
    plt.show()

def plotVaryingDiffDegree(data):
    steps = data['1']['DPIterations'][0]
    runs_processed = []

    for i in data:
        run = data[i]
        runs_processed.append([])
        conn_count = run['OriginalRun']['RunHistory']['ConnectionCount']
        for diff in range(len(run['DiffusionSteps'])):
            curr_run = run['DiffusionSteps'][str(diff)]
            val_split = curr_run['SubcloudRuns']['0']["ValidationSplit"]
            merge_err = curr_run['MergedTotalError']
            runs_processed[int(i) - 1].append(math.log(merge_err / (val_split * conn_count)))

    for i in range(len(runs_processed)):
        plt.plot(range(data[str(i+1)]['MaxSteps'] + 1), runs_processed[i], linestyle='-', marker='o', label=f"Diff. Degree {i+1}")
    
    plt.legend()
    plt.show()

def PlotAllPlots(file_name):
    for i in range(1):
        plot2DErrorOverTime(file_name, i)
        plot2DValErrorOverTime(file_name, i)
    Plot2DDiffValOriginalVSMerged(file_name)
    Plot2DDiffOriginalVSMerged(file_name)
    Plot2DMergedErrorDiff(file_name)

if __name__ == "__main__":
    #for i in range(8):
        #plot2DValErrorOverTime("movielens_data_points_hist_diff_simple_t4.json", i)
    #Plot2DDiffOriginalVSMerged("movielens_data_points_hist_diff_simple_t8.json")
    #Plot2DMergedErrorDiff("movielens_data_points_hist_diff_simple_t8.json")
    plotVaryingDiffDegree(util.load_json(util.get_path_to_run_history_file("ml_hist_vary_diff_degree_t1.json")))
    # PlotAllPlots("ml_hist_diff_static_lr_root2base_ite_t1.json")