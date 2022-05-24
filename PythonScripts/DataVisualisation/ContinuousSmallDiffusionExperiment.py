#!/usr/bin/python
# -*- coding: latin-1 -*-import unicodedat

from cmath import inf
import plotting_util as util
import matplotlib.pyplot as plt
import matplotlib as mpl
import numpy as np;
import matplotlib.cm as cm
import math
import argparse
from plotting_util import get_path_to_run_history_file, load_json

from pathlib import Path

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

IS_DOUBLE_DIPPED = False

parser = argparse.ArgumentParser()
parser.add_argument('-q', dest="quiet", action="store_true", help="indicates whether to print only final results")
parser.add_argument('-c', dest="combine", action="store_true", help="flag for combining 2 files into plots")
parser.add_argument('--type', dest="type", type=str, help="the name of the experiment to plot")
parser.add_argument('--constant', dest="constant", type=float, nargs='+', help="the constant(s) of the lr function")
parser.add_argument('--factor', dest="factor", type=float, nargs='+', help="the factor(s) of the lr function")
parser.add_argument('--base', dest="base", type=float, default=0.1, help="if the base is constant, set here")
parser.add_argument('--files', dest='files', type=str, nargs='+', help='name of the files to use')
args = parser.parse_args()

def load(file_name):
    path = get_path_to_run_history_file(file_name)
    experiment_results = load_json(path)
    if (not args.quiet):
        print(experiment_results)
    return experiment_results

def get_overview(data):
    number_of_runs = 0

    print(f"len(data) = {len(data)}")
    for i in data:
        print(f"\tlen(data[{i}]) = {len(data[i])}")
        
        for j in data[i]:
            print(f"\t\tlen(data[{i}][{j}]) = {len(data[i][j])}")
            # print(f"           merge_LR = {data[str(i)][str(j)]['MergeLR']}")

            number_of_runs += 1
    
    print(f"number_of_runs = {number_of_runs}")
    

def plot_LR_and_decay(data):
    first_LRs = get_first_learning_rates(data)
    LR_decays = get_learning_rate_decays(data)

    heights = [[], []]
    heights = get_error_and_val_error(data)

    mini = (inf, -1, -1)
    for i in range(len(heights[1])):
        for j in range(len(heights[1][i])):
            mini = mini if (mini[0] < heights[1][i][j]) else (heights[1][i][j], i, j)
            if (not args.quiet):
                print(heights[1][i][j])

    print(f"\n\nMin is {mini}, with lr: {first_LRs[mini[1]]} and decay: {LR_decays[mini[2]]}")
    util.plotSurface(heights, "ln Error ln(e)", first_LRs, "First Learning Rate", 
                     LR_decays, "Learning Rate Decay", 2, ["MSE", "Validation MSE"])

def plot_LRDecay_Constant_Factor(data):
    constants = get_decay_constants(data)
    factors = get_decay_factors(data, args.constant[0])

    heights = [[], []]
    heights = get_error_and_val_error(data)

    mini = (inf, -1, -1)
    for i in range(len(heights[1])):
        for j in range(len(heights[1][i])):
            mini = mini if (mini[0] < heights[1][i][j]) else (heights[1][i][j], i, j)
            
            if (not args.quiet):
                print(heights[1][i][j])

    print(f"\n\nMin is {mini}, with constant: {constants[mini[1]]} and factor: {factors[mini[2]]}")
    util.plotSurface(heights, "ln Error ln(e)", constants, "Constant g", 
                     factors, "Factor b", 2, ["MSE", "Validation MSE"])

def plot_LRDecay_Constant(data):
    constants = get_decay_experiment_constants(data)
    (val_errors, min_index) = get_decay_experiment_val_errors(data)

    print(f"Minimum val error at constant {constants[min_index]} (index {min_index}) with val error {val_errors[min_index]}")

    plt.plot([float(x) for x in constants], val_errors, linestyle='-', marker='o')

    plt.title("RMSE by decay constant")
    plt.xlabel("Constant g")
    plt.ylabel("Validation RMSE")
    plt.legend()

    plt.show()

def combined_plot_LRDecay_Constant(data1, data2):
    constants1 = get_decay_experiment_constants(data1)
    (val_errors1, min_index1) = get_decay_experiment_val_errors(data1)
    constants2 = get_decay_experiment_constants(data2)
    (val_errors2, min_index2) = get_decay_experiment_val_errors(data2)

    print(f"For data1: Minimum val error at constant {constants1[min_index1]} (index {min_index1}) with val error {val_errors1[min_index1]}")
    print(f"For data2: Minimum val error at constant {constants2[min_index2]} (index {min_index2}) with val error {val_errors2[min_index2]}")

    plt.plot([float(x) for x in constants1][1:9], val_errors1[1:9], linestyle='-', marker='o', label="MovieLens")
    plt.plot([float(x) for x in constants2][2:24], val_errors2[2:24], linestyle='-', marker='o', label="Douban")

    plt.title("RMSE by decay constant")
    plt.xlabel("Constant g")
    plt.ylabel("Validation RMSE")
    plt.legend()

    plt.show()

def plot_LRDecay_Factor(data):
    factors = get_decay_experiment_factors(data, args.constant[0])
    (val_errors, min_index) = get_decay_experiment_val_errors(data)

    print(f"Minimum val error at factor {factors[min_index]} (index {min_index}) with val error {val_errors[min_index]}")

    plt.plot([float(x) for x in factors], val_errors, linestyle='-', marker='o')

    plt.title("RMSE by decay factor")
    plt.xlabel("Factor b")
    plt.ylabel("Validation RMSE")
    plt.legend()
    plt.show()

def combined_plot_LRDecay_Factor(data1, data2):
    factors1 = get_decay_experiment_factors(data1, args.constant[0])
    (val_errors1, min_index1) = get_decay_experiment_val_errors(data1)
    factors2 = get_decay_experiment_factors(data2, args.constant[1])
    (val_errors2, min_index2) = get_decay_experiment_val_errors(data2)

    print(f"Data1: Minimum val error at factor {factors1[min_index1]} (index {min_index1}) with val error {val_errors1[min_index1]}")
    print(f"Data2: Minimum val error at factor {factors2[min_index2]} (index {min_index2}) with val error {val_errors2[min_index2]}")

    plt.plot([float(x) for x in factors1], val_errors1, linestyle='-', marker='o', label="MovieLens")
    plt.plot([float(x) for x in factors2], val_errors2, linestyle='-', marker='o', label="Douban")

    plt.title("RMSE by decay factor")
    plt.xlabel("Factor b")
    plt.ylabel("Validation RMSE")
    plt.legend()
    plt.show()

def plot_LRDecay_Base(data):
    bases = get_decay_experiment_bases(data, args.constant[0], args.factor[0])
    (val_errors, min_index) = get_decay_experiment_val_errors(data)

    print(f"Minimum val error at base {bases[min_index]} (index {min_index}) with val error {val_errors[min_index]}")

    plt.plot([float(x) for x in bases], val_errors, linestyle='-', marker='o')

    plt.title("RMSE by decay base")
    plt.xlabel("Base a")
    plt.ylabel("Validation RMSE")
    plt.legend()
    plt.show()

def combined_plot_LRDecay_Base(data1, data2):
    bases1 = get_decay_experiment_bases(data1, args.constant[0], args.factor[0])
    (val_errors1, min_index1) = get_decay_experiment_val_errors(data1)
    bases2 = get_decay_experiment_bases(data2, args.constant[1], args.factor[1])
    (val_errors2, min_index2) = get_decay_experiment_val_errors(data2)

    print(f"Data1: Minimum val error at base {bases1[min_index1]} (index {min_index1}) with val error {val_errors1[min_index1]}")
    print(f"Data2: Minimum val error at base {bases2[min_index2]} (index {min_index2}) with val error {val_errors2[min_index2]}")

    plt.plot([float(x) for x in bases1], val_errors1, linestyle='-', marker='o', label="MovieLens")
    plt.plot([float(x) for x in bases2], val_errors2, linestyle='-', marker='o', label="Douban")

    plt.title("RMSE by decay base")
    plt.xlabel("Base a")
    plt.ylabel("Validation RMSE")
    plt.legend()
    plt.show()

# utility functions for extraction
def get_decay_experiment_constants(data):
    constants = []
    for i in data:
        constants.append(data[i]['DiffusionSteps']['0']['SubcloudRuns']['0']['RunHistory']['LR'] - args.factor)
    
    return constants

def get_decay_experiment_factors(data, constant):
    factors = []
    for i in data:
        factors.append(data[i]['DiffusionSteps']['0']['SubcloudRuns']['0']['RunHistory']['LR'] - constant)

    return factors

def get_decay_experiment_bases(data, constant, factor):
    bases = []
    for i in data:
        bases.append((data[i]['DiffusionSteps']['1']['SubcloudRuns']['0']['RunHistory']['LR'] - constant) / factor)

    return bases

def get_decay_factors(data):
    factors = []
    for i in data:
        factors.append(data[i]['0']['DiffusionSteps']['0']['SubcloudRuns']['0']['RunHistory']['LR'])
    
    return factors

def get_decay_constants(data):
    constants = []
    first_run = data['0']
    for i in first_run:
        constants.append(first_run[i]['DiffusionSteps']['0']['SubcloudRuns']['0']['RunHistory']['LR'])
    
    return constants
# lr(i, ii) = ii * g + i * b *a^s
def get_first_learning_rates(data):
    first_learing_rates = []
    for i in data:
        first_LR = data[i]['0']['DiffusionSteps']['0']['SubcloudRuns']['0']['RunHistory']['LR']
        first_learing_rates.append(first_LR)
        if (not args.quiet):
            print(f"i = {i}")
            print(f"\tfirst learning rate = {first_LR}")

    if (not args.quiet):
        print(f" learning rates = {first_learing_rates}")
    return first_learing_rates

def get_learning_rate_decays(data):
    learning_rate_decays = []
    for j in data:
        first_LR = data['1'][j]['DiffusionSteps']['0']['SubcloudRuns']['0']['RunHistory']['LR']
        second_LR = data['1'][j]['DiffusionSteps']['1']['SubcloudRuns']['0']['RunHistory']['LR']
        LR_decay = (second_LR - args.constant) / (first_LR - args.constant)
        learning_rate_decays.append(LR_decay)
        
        if (not args.quiet):
            print(f"\tfirst learning rate = {first_LR}")
            print(f"\tsecond learning rate = {second_LR}")
            print(f"\t\tlearning rate decay = {LR_decay}")

    if (not args.quiet):
        print(f" learning rate decays = {learning_rate_decays}")
    return learning_rate_decays

def get_decay_experiment_val_errors(data):
    val_errors = [] 
    min_verror = inf
    min_verror_index = -1

    for i in data:
        val_split = data[i]["MergedRun"]["ValidationSplit"]
        hist = data[i]["MergedRun"]["RunHistory"]
        connection_count = hist["ConnectionCount"]
        last_val_error = hist["ValError"][0]

        doubleDipFactor =  0.5 if IS_DOUBLE_DIPPED else 1
        val_error = math.sqrt(doubleDipFactor * last_val_error / (val_split * connection_count))

        if (val_error < min_verror):
            min_verror = val_error
            min_verror_index = int(i)

        val_errors.append(val_error)

    if (not args.quiet):
        print(f"Minimum validation error: {min_verror}, at {min_verror_index}")

    return (val_errors, min_verror_index)

def get_error_and_val_error(data):
    errors = [[], []] 
    min_verror = inf
    min_verror_index = (-1, -1)

    for i in data:
        errors[0].append([])
        errors[1].append([])
        for j in data[i]:
            val_split = data[i][j]["MergedRun"]["ValidationSplit"]
            hist = data[i][j]["MergedRun"]["RunHistory"]
            connection_count = hist["ConnectionCount"]
            last_error = hist["Error"][0]
            last_val_error = hist["ValError"][0]

            doubleDipFactor =  0.5 if IS_DOUBLE_DIPPED else 1
            error = math.log(doubleDipFactor * last_error / ((1 - val_split) * connection_count))
            val_error = math.log(doubleDipFactor * last_val_error / (val_split * connection_count))

            if (val_error < min_verror):
                min_verror = val_error
                min_verror_index = (i, j)

            errors[0][int(i)].append(error)
            errors[1][int(i)].append(val_error)

    if (not args.quiet):
        print(f"Minimum validation error: {min_verror}, at {min_verror_index}")

    return errors


def plotDPMachineProgress(data, x_index, y_index, machine=0):
    target = data[str(x_index)][str(y_index)]
    steps = range(len(target["DiffusionSteps"]))

    run = [target["DiffusionSteps"][str(i)]["MergedTotalError"] for i in steps]
    run = [target["OriginalRun"]["RunHistory"]["Error"][0]/2] + run
    run_processed = [math.log(err / target["ConnectionsAfterMerge"]) for err in run]

    
    plt.plot(range(len(run)), run_processed, linestyle='-', marker='o', label="Merge Error")
    
    plt.legend()
    plt.show()

def plot2DValErrorOverTime(file_name, index_x, index_y, machine, degree=4):
    cut = 2
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    steps = [range(len(metrics[str(index_x)][str(index_y)]["DiffusionSteps"][str(i)]["SubcloudRuns"]["0"]["RunHistory"]["ValError"])) for i in range(cut, len(metrics[str(index_x)][str(index_y)]["DiffusionSteps"]))]
    runs = [metrics[str(index_x)][str(index_y)]["DiffusionSteps"][str(i)]["SubcloudRuns"][str(machine)] for i in range(cut, len(metrics[str(index_x)][str(index_y)]["DiffusionSteps"]))]
    run_names = [f"D{degree}M{machine}_Step{i+1}" for i in range(cut, len(metrics[str(index_x)][str(index_y)]["DiffusionSteps"]))]
    runs_processed = []

    for run in runs:
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        runs_processed.append([math.log(err / (val_split * conn_count)) for err in run["RunHistory"]["ValError"]])

    for i in range(len(runs_processed)):
        plt.plot(steps[i], runs_processed[i], linestyle='-', marker='o', label=run_names[i])
    
    plt.legend()
    plt.show()

def plot2DValErrorOverTimeForLongDiff(file_name, machine, degree=4):
    cut = 1
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    steps = [range(findZeroLength(metrics["DiffusionSteps"][str(i)]["SubcloudRuns"][str(machine)]["RunHistory"]["ValError"])) for i in range(cut, len(metrics["DiffusionSteps"]))]
    runs = [metrics["DiffusionSteps"][str(i)]["SubcloudRuns"][str(machine)] for i in range(cut, len(metrics["DiffusionSteps"]))]
    run_names = [f"D{degree}M{machine}_Step{i+1}" for i in range(cut, len(metrics["DiffusionSteps"]))]
    runs_processed = []

    for i in range(len(runs)):
        run = runs[i]
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        runs_processed.append([math.log(run["RunHistory"]["ValError"][j] / (val_split * conn_count)) for j in range(steps[i][-1]+1)])

    for i in range(len(runs_processed)):
        plt.plot(steps[i], runs_processed[i], linestyle='-', marker='o', label=run_names[i])
    
    plt.legend()
    plt.show()

def plotDPMachineProgressLongDiff(data, machine=0):
    steps = range(len(data["DiffusionSteps"]))

    run = [data["DiffusionSteps"][str(i)]["MergedTotalValError"] for i in steps]
    run = [data["OriginalRun"]["RunHistory"]["Error"][0]] + run
    run_processed = [math.log(err / (data["ConnectionsAfterMerge"] * data["OriginalRun"]["ValidationSplit"])) for err in run]

    
    plt.plot(range(len(run)), run_processed, linestyle='-', marker='o', label="Merge Error")
    
    plt.legend()
    plt.show()

def findZeroLength(list):
    entries = 0
    for i in range(len(list)):
        if (list[i] == 0):
            return entries
        entries += 1
    return entries

if __name__ == "__main__":
    if (args.files is not None):
        data = load(args.files[0])
        if (args.type == "fb"):
            plot_LR_and_decay(data)
        elif (args.type == "cf"):
            plot_LRDecay_Constant_Factor(data)
        elif (args.type == "c"):
            assert args.factor is not None
            assert args.base is not None
            combined_plot_LRDecay_Constant(data, load(args.files[1])) if (args.combine and len(args.files) > 1) else plot_LRDecay_Constant(data)
        elif (args.type == "f"):
            assert args.constant is not None
            assert args.base is not None
            combined_plot_LRDecay_Factor(data, load(args.files[1])) if (args.combine and len(args.files) > 1) else plot_LRDecay_Factor(data)
        elif (args.type == "b"):
            assert args.factor is not None
            assert args.constant is not None
            combined_plot_LRDecay_Base(data, load(args.files[1])) if (args.combine and len(args.files) > 1) else plot_LRDecay_Base(data)
    else:
        plot_LRDecay_Constant_Factor(load(args.files[0]))
    # get_overview()
    # plot_LR_and_decay()
    #plot2DValErrorOverTime("ml_hist_cont_sml_diff1_zoomed_t3.json", 1, 6, 0, 3)
    #plot_LR_and_decay()
    #plotDPMachineProgress(load(), 1, 7)
    #plotDPMachineProgressLongDiff(load())
