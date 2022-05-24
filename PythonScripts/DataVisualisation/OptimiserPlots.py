#!/usr/bin/python
# -*- coding: latin-1 -*-import unicodedat

import plotting_util as util
import matplotlib.pyplot as plt
import math


from pathlib import Path

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

from scipy import stats
import numpy as np

import argparse

parser = argparse.ArgumentParser()
parser.add_argument('-q', dest="quiet", action="store_true", help="indicates whether to print only final results")
parser.add_argument('-u', dest="use_args", action="store_true",)
parser.add_argument('--adam_lr', dest="adam_lr", type=float)
parser.add_argument('--adam_f', dest="adam_f", type=str)
parser.add_argument('--rmsprop_lr', dest="rmsprop_lr", type=float)
parser.add_argument('--rmsprop_f', dest="rmsprop_f", type=str)
parser.add_argument('--nesterov_lr', dest="nesterov_lr", type=float)
parser.add_argument('--nesterov_f', dest="nesterov_f", type=str)
parser.add_argument('--momentum_lr', dest="momentum_lr", type=float)
parser.add_argument('--momentum_f', dest="momentum_f", type=str)
parser.add_argument('--vanilla_lr', dest="vanilla_lr", type=float)
parser.add_argument('--vanilla_f', dest="vanilla_f", type=str)
args = parser.parse_args()

def mean_confidence_interval(data, confidence=0.95):
    """Stolen from stack-overflow"""
    a = np.array(data, dtype=np.float32)
    n = len(a)
    m, se = np.mean(a), stats.sem(a)
    h = se * stats.t.ppf((1 + confidence) / 2., n-1)

    return m, h

def plotVErrorByLR(file_name):
    metrics = util.load_json(util.get_path_to_run_history_file(file_name))
    metrics = {float(key): item for key, item in metrics.items()}
    metrics = dict(sorted(metrics.items(), key=lambda item: item[0]))

    steps = []

    run_v_error = []

    for key in metrics:
        run = metrics[key]
        val_split = run["ValidationSplit"]
        conn_count = run["RunHistory"]["ConnectionCount"]
        error = float(run["RunHistory"]["ValError"][-1])
        log_error = math.log(error / (val_split * conn_count))
        if log_error < 10 and error != 0 and not math.isnan(error):
            run_v_error.append(log_error)
            steps.append(float(key))

    plt.plot(steps, run_v_error, linestyle='-', marker='o', label=file_name.split("_")[-1].split(".")[0])
    
    plt.legend()
    plt.show()

def printMinErrors(fileNamesDict):
    for optimiser in fileNamesDict:
        file_name = fileNamesDict[optimiser]
        metrics = util.load_json(util.get_path_to_run_history_file(file_name))

        metrics = dict(sorted(metrics.items(), key=lambda item: item[0]))

        min_mse = 1337e10
        min_v_mse = 1337e10
        min_key = ""
        min_v_key = ""

        for key in metrics:
            run = metrics[key]
            val_split = run["ValidationSplit"]
            conn_count = run["RunHistory"]["ConnectionCount"]
            v_error = float(run["RunHistory"]["ValError"][-1])
            error = float(run["RunHistory"]["Error"][-1])
            mse = (error / ((1 - val_split) * conn_count))
            v_mse = (v_error / (val_split * conn_count))

            if mse < min_mse:
                min_mse = mse
                min_key = key
            if v_mse < min_v_mse:
                min_v_mse = v_mse
                min_v_key = key
        print(f"{optimiser}:{' ' * (15 - len(optimiser))}  mse: {format(min_mse)} {' ' * 4}" 
                f"rms: {format(math.sqrt(min_mse))} at lr = {min_key}"
                f" \n{' ' * 12}| val mse: {format(min_v_mse)} "
                f"val rms: {format(math.sqrt(min_v_mse))} at lr = {min_v_key}\n")        

def printMinErrorsConfidenceInterval(fileNamesDict):
    
    print(f"{' ' * 11} mse {' ' * 11} rms {' ' * 11} val mse {' ' * 8} val rms {' ' * 7} lr")
    for optimiser in fileNamesDict:
        file_name, lr = fileNamesDict[optimiser]
        metrics = util.load_json(util.get_path_to_run_history_file(file_name))
        #metrics_2 = util.load_json(util.get_path_to_run_history_file(file_name[:-5] + "_2.json"))
        #metrics |= {str(int(key) + len(metrics)):value for (key, value) in metrics_2.items()}

        mse_array = []
        mse_v_array = []

        for key in metrics:
            run = metrics[key]
            val_split = run["ValidationSplit"]
            conn_count = run["RunHistory"]["ConnectionCount"]
            v_error = float(run["RunHistory"]["ValError"][-1])
            error = float(run["RunHistory"]["Error"][-1])
            mse = (error / ((1 - val_split) * conn_count))
            v_mse = (v_error / (val_split * conn_count))

            mse_v_array.append(v_mse)
            mse_array.append(mse)

        confidence = 0.95
        mean, interval = mean_confidence_interval(mse_array, confidence)
        v_mean, v_interval = mean_confidence_interval(mse_v_array, confidence)
        r_mean, r_interval = mean_confidence_interval([math.sqrt(num) for num in mse_array], confidence)
        r_v_mean, r_v_interval = mean_confidence_interval([math.sqrt(num) for num in mse_v_array], confidence)


        print(f"{optimiser}{' ' * (10 - len(optimiser))}| {formatI(mean, interval)}" 
                f" | {formatI(r_mean, r_interval)}"
                f" | {formatI(v_mean, v_interval)} "
                f" | {formatI(r_v_mean, r_v_interval)} | {lr}")

def formatI(num, interval):
    return ("%.3f" % num) + " \u00B1 " + ("%.3f" % interval)

def format(num):
    return "%.3f" % num

if __name__ == "__main__":
    if (args.use_args):
        printMinErrorsConfidenceInterval({"Adam": (args.adam_f, args.adam_lr),
                                      "RMSProp": (args.rmsprop_f, args.rmsprop_lr),
                                      "Nesterov": (args.nesterov_f, args.nesterov_lr),
                                      "Momentum": (args.momentum_f, args.momentum_lr),
                                      "Vanilla": (args.vanilla_f, args.vanilla_lr)})
    else:
        printMinErrorsConfidenceInterval({"Adam": ("dn_hist_vary_lr_Adam.json", 0.364),
                                      "RMSProp": ("dn_hist_vary_lr_RMSProp.json", 1.72),
                                      "Nesterov": ("dn_hist_vary_lr_Nesterov.json", 0.00168),
                                      "Momentum": ("dn_hist_vary_lr_Momentum.json", 0.0010),
                                      "Vanilla": ("dn_hist_vary_lr_Vanilla.json", 0.02494)})



    #plotVErrorByLR("dn_hist_vary_lr_highdim_Momentum.json")
    #printMinErrors({"Adam": "ml_hist_vary_lr_adam2.json", 
    #                "Momentum": "ml_hist_vary_lr_momentum1.json",
    #                "Nesterov": "ml_hist_vary_lr_nesterov.json",
    #                "Vanilla": "ml_hist_vary_lr_vanilla7.json",
    #                "RMSProp": "ml_hist_vary_lr_RMSProp_repeating.json"})