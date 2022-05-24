from cProfile import label
from webbrowser import get
import plotting_util as util
import matplotlib.pyplot as plt
import matplotlib as mpl
import numpy as np;
import matplotlib.cm as cm
import math

from plotting_util import get_path_to_run_history_file, load_json

from pathlib import Path

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

def plot2DErrorOverTime(file_name):
    metrics = load_json(get_path_to_run_history_file(file_name))
    steps = range(len(metrics["OriginalRun"]["RunHistory"]["Error"]))

    run_names = ["OriginalRun", "MergedRun", "SubcloudRun1", "SubcloudRun2"]
    runs = [metrics["OriginalRun"], metrics["MergedRun"], metrics["SubcloudRuns"][0], metrics["SubcloudRuns"][1]]
    runs_processed = []

    for run in runs:
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        runs_processed.append([math.log(err / ((1-val_split) * conn_count)) for err in run["RunHistory"]["Error"]])

    for i in range(len(runs_processed)):
        plt.plot(steps, runs_processed[i], linestyle='-', marker='o', label=run_names[i])
    
    plt.legend()
    plt.show()

def plot2DValErrorOverTime(file_name):
    metrics = load_json(get_path_to_run_history_file(file_name))
    steps = range(len(metrics["OriginalRun"]["RunHistory"]["ValError"]))

    run_names = ["OriginalRun", "MergedRun", "SubcloudRun1", "SubcloudRun2"]
    runs = [metrics["OriginalRun"], metrics["MergedRun"], metrics["SubcloudRuns"][0], metrics["SubcloudRuns"][1]]
    runs_processed = []

    for run in runs:
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        runs_processed.append([math.log(err / (val_split * conn_count)) for err in run["RunHistory"]["ValError"]])

    for i in range(len(runs_processed)):
        plt.plot(steps, runs_processed[i], linestyle='-', marker='o', label=run_names[i])
    
    plt.legend()
    plt.show()


if __name__ == "__main__":
    #plot2DErrorOverTime("movielens_data_points_hist_spl_vary_merge_lr_t1.json")
    plot2DValErrorOverTime("movielens_data_points_hist_spl_vary_merge_lr_t1.json")