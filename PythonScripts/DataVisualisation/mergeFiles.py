from cmath import inf
import plotting_util as util
import matplotlib.pyplot as plt
import matplotlib as mpl
import numpy as np;
import matplotlib.cm as cm
import math
from plotting_util import get_path_to_run_history_file, load_json
import json 
from pathlib import Path

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

def load(file):
    path = get_path_to_run_history_file(file)
    experiment_results = load_json(path)
    return experiment_results

first_data = load("ml1m_tune_decay_constantq_hist2_.json")
second_data = load("ml1m_tune_decay_constant_hist2_.json")

counter = len(first_data)
for i in second_data:
    shifted_i = str(int(i) + counter)
    first_data[shifted_i] = second_data[i]
    counter += 1

constants = []
for i in first_data:
    constants.append(first_data[i]['DiffusionSteps']['0']['SubcloudRuns']['0']['RunHistory']['LR'] - 0.025)

path = get_path_to_run_history_file("ml_super_merged_file.json")
print(f"Writing data to path '{path}'")
with open(path, 'w') as file:
    json.dump(first_data, file, indent=2)

pass

