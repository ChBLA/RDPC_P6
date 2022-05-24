import plotting_util as util
import matplotlib.pyplot as plt
import matplotlib as mpl
import numpy as np;
import matplotlib.cm as cm
import math
from matplotlib.colors import LightSource

def plotHistorySurface(file_name):
    metrics = util.load_json(file_name)
    mpl.rcParams['legend.fontsize'] = 10

    connections = metrics["Connections"]
    histories = metrics["Histories"]

    dimensions = range(0, len(histories))
    steps = range(len(histories[str(0)]["Error"]))

    dimensions, steps = np.meshgrid(dimensions, steps)

    def getError(x, y):
        return math.log(histories[str(x)]["Error"][y] / (0.7 * connections))

    def getValError(x, y):
        return math.log(histories[str(x)]["ValError"][y] / (0.3 * connections))

    dim_rav = np.ravel(dimensions)
    step_rav = np.ravel(steps)

    errors = np.array([getError(dim_rav[i], step_rav[i]) for i in range(len(dim_rav))])
    val_errors = np.array([getValError(dim_rav[i], step_rav[i]) for i in range(len(dim_rav))])

    errors = errors.reshape(dimensions.shape)
    val_errors = val_errors.reshape(dimensions.shape)

    fig = plt.figure()
    axe = plt.axes(projection='3d')

    surf1 = axe.plot_surface(dimensions, steps, errors, alpha = 1, rstride=1, cstride=1, linewidth=0.0, antialiased=False, color=np.array((0, 0, 1, 1)), label = "MSE")
    surf2 = axe.plot_surface(dimensions, steps, val_errors, alpha = 1, rstride=1, cstride=1, linewidth=0.0, antialiased=False, color=np.array((1, 0, 0, 1)), label = "Validation MSE")

    surf1._facecolors2d=surf1._facecolor3d
    surf2._facecolors2d=surf2._facecolor3d
    surf1._edgecolors2d=surf1._edgecolor3d
    surf2._edgecolors2d=surf2._edgecolor3d

    axe.set_xlabel(format("Dimensions d"))
    axe.set_ylabel(format("Step s"))
    axe.set_zlabel("log(MSE)")

    axe.legend()

    plt.show()

def plotHistoryPoints(file_name, val=False):
    metrics = util.load_json(file_name)
    mpl.rcParams['legend.fontsize'] = 10

    connections = metrics["Connections"]
    histories = metrics["Histories"]

    fig = plt.figure()
    axe = plt.axes(projection='3d')
    
    COLOR = cm.rainbow(np.linspace(0, 1, len(histories) * 2))
    for i, history in enumerate(histories):
        error = [math.log(v / (0.7 * connections)) for v in history["Error"]]
        val_error = [math.log(v / (0.3 * connections)) for v in history["ValError"]]
        length = len(error)
        axe.plot([i + 1] * length, range(length), error, "o", color=COLOR[i], label=f"{i + 1} dimensions")
        axe.plot([i + 1] * length, range(length), val_error, "o", color=COLOR[int(i + len(histories))], label=f"{i + 1} dimensions")

    axe.set_xlabel(format("Dimensions d"))
    axe.set_ylabel(format("Step s"))
    axe.set_zlabel("log(MSE)")

    #axe.legend()

    plt.show()
