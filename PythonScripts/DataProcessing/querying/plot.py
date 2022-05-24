import matplotlib.pyplot as plt
import numpy as np
import matplotlib.cm as cm

def getProjetionLength(vector, onto):
    norm_squared = sum(onto**2) 
    dist = (np.dot(vector, onto) / norm_squared)
    return dist

def getPoints(getPos, allNames, origin, basisX, basisY, count):
    x = []
    y = []
    names = []
    candidates = []

    for name in allNames:
        other = getPos(name) - origin
        xPos = getProjetionLength(other, basisX)
        yPos = getProjetionLength(other, basisY)
        dist = np.linalg.norm(other - (basisX * xPos + basisY * yPos))
        candidates.append((dist, xPos, yPos, name))

    candidates = sorted(candidates, key = lambda candidate : candidate[0])[:count]

    for candidate in candidates:
        names.append(candidate[3])
        x.append(candidate[1])
        y.append(candidate[2])

    return (x, y, names)

def plot(getPos, allnames, name1, name2, name3):

    o = getPos(name1)
    v = getPos(name2) - o
    u = getPos(name3) - o

    v_norm_squared = sum(v**2) 
    proj_of_u_on_v_proportion = (np.dot(u, v)/v_norm_squared)
    anti_proj_of_u_on_v = u - proj_of_u_on_v_proportion * v

    x, y, names = getPoints(getPos, allnames, o, v, anti_proj_of_u_on_v, 20)

    _ , ax = plt.subplots()
    source = [name1, name2, name3]
    COLOR = [np.array((1, 0, 0, 1)) if names[i] in source else np.array((0, 0, 0.5, 1)) for i in range(len(x))]
    
    ax.scatter(x, y, color = COLOR)

    for i, name in enumerate(names):
        ax.annotate(name, (x[i], y[i]))

    plt.show()

    