import json
import matplotlib.pyplot as plt
import matplotlib as mpl
import numpy as np;
import matplotlib.cm as cm
import math
from pathlib import Path

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

def load_json(path):
    with open(path, "r") as file:
        return json.loads(file.read())

def save_json(path, object):
    with open(path, "w") as file:
        json.dump(object, file)

def get_path_to_data_file(file_name):
    return Path.joinpath(ph.data_dir_path, file_name)

def get_path_to_run_history_file(file_name):
    return Path.joinpath(ph.data_run_hist_path, file_name)

def plotSurface(heights, zTitle, xAxis, xTitle, yAxis, yTitle, num_of_surfaces, surfaceLabels):
    mpl.rcParams['legend.fontsize'] = 10

    xIndices = range(len(xAxis))
    yIndices = range(len(yAxis))

    xIndices, yIndices = np.meshgrid(xIndices, yIndices)
    xAxis, yAxis = np.meshgrid(xAxis, yAxis)

    x_rav = np.ravel(xIndices)
    y_rav = np.ravel(yIndices)

    total_range = range(len(x_rav))

    height_rav = []

    for surface in range(num_of_surfaces):
        height_rav.append(np.array([heights[surface][x_rav[i]][y_rav[i]] for i in total_range]))
        height_rav[surface] = height_rav[surface].reshape(xIndices.shape)
    
    COLOR = cm.rainbow(np.linspace(0, 1, num_of_surfaces))
    fig = plt.figure()
    axe = plt.axes(projection='3d')

    for surface in range(num_of_surfaces):
        surf = axe.plot_surface(xAxis, yAxis, height_rav[surface], alpha = 1, rstride=1, cstride=1, linewidth=0.0, 
                                antialiased=False, color=COLOR[surface], label = surfaceLabels[surface])
        surf._facecolors2d=surf._facecolor3d
        surf._edgecolors2d=surf._edgecolor3d

    axe.set_xlabel(xTitle)
    axe.set_ylabel(yTitle)
    axe.set_zlabel(zTitle)

    axe.legend()

    plt.show()

def mergeData(path1, path2, destinationPath):
    metrics1 = load_json(path1)
    metrics2 = load_json(path2)

    if(metrics1["Connections"] != metrics2["Connections"]):
        raise Exception("Incompatible point clouds")

    results = {}
    metricsOutput = {"Connections" : metrics1["Connections"], "AllResults" : results}

    for dim_key in metrics1["AllResults"].keys():
        results[dim_key] = {}
        dim1 = metrics1["AllResults"][dim_key]
        dim2 = metrics2["AllResults"][dim_key]
        length = len(dim1)
        for i in range(length):
            results[dim_key][str(i)] = dim1[str(i)]

        length2 = length + len(dim2)
        for i in range(length, length2):
            results[dim_key][str(i)] = dim2[str(i - length)]

    save_json(destinationPath, metricsOutput)
    # ...