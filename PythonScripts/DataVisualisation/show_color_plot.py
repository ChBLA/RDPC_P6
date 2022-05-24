import json
import matplotlib.pyplot as plt
import numpy as np
import math
import matplotlib as mpl
import matplotlib.cm as cm
from pathlib import Path

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()


def show_color_plot():
    file_path = Path.joinpath(ph.data_point_cloud_path, "color_data.json")
    print(f"file_path: {file_path}")

    file = open(file_path)
    data = json.load(file)
    print(len(data["Positions"]))
    file.close()
    print(data)

    mpl.rcParams['legend.fontsize'] = 10

    fig = plt.figure()
    axe = fig.gca(projection='3d')

    for i in range(len(data["Positions"])):
        axe.plot(data["Positions"][i]["Item1"], data["Positions"][i]["Item2"], data["Positions"][i]["Item3"],
            "o", color=tuple([data["Colors"][i]["Item1"]/255, data["Colors"][i]["Item2"]/255, data["Colors"][i]["Item3"]/255]))

    axe.legend("sup")
    plt.show()


if __name__ == "__main__":
    show_color_plot()

