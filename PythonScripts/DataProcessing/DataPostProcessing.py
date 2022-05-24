import json
import matplotlib.pyplot as plt
import numpy as np
import math
import matplotlib as mpl
import matplotlib.cm as cm
import os
import copy


def main():
    fileDir = os.path.dirname(os.path.realpath('__file__'))
    fileName = "movielens_data_points.json"
    path = os.path.join(fileDir, "DataVisualisation", "data", fileName)

    file = open(path)
    data = json.load(file)
    print(len(data["Positions"]))
    file.close()

    plot_data = np.transpose(data["Positions"])
    #print(data)
    dims = 10

    matrix = np.array([row[:dims-1] + [1] for row in data["Positions"]])
    matrix_transposed = np.transpose(matrix)
    last_col = np.array([row[-1] for row in data["Positions"]])
    y_col = matrix_transposed.dot(last_col)
    matrix = matrix_transposed.dot(matrix)
    matrix_p = np.hstack((matrix, np.array([[v] for v in y_col])))
    res = gaussian_elimination(matrix_p)


    # Centering all dims at 0

    tester = np.empty(shape=(2,5))
    print(len(tester))

    example = np.array([[1,3,1,9],[1,1,-1,1],[3,11,5,35]])
    ans = gaussian_elimination(example)
    print(ans)




def gaussian_elimination(matrix):
    M = copy.deepcopy(matrix)
    for i in range(len(M)):
        if M[i][i] != 0:
            M[i] = M[i] / M[i][i]
        
        for k in range(len(M)):
            if k != i:
                M[k] = M[k] - M[i] * M[k][i]
    return np.transpose(M)[-1]

def plot_boxplot(data, dims):
    fig, axes = plt.subplots(1, dims)

    for i in range(dims):
        axes[i].boxplot(data[i], 0, '')
        axes[i].set_ylim(-3, 3)
        axes[i].set_title(f"dim {i + 1}")

    fig.subplots_adjust(left=0.08, right=0.98, bottom=0.05, top=0.9,
                        hspace=0.4, wspace=1)
    plt.show()



def center_data_at_origin(data, dims):
    for i in range(dims):
        avg = np.average(data[i])
        for j in range(len(data[i])):
            data[i][j] = data[i][j] - avg

if __name__ == "__main__":
    main()
