import numpy as np
import math

def getPosition(position_by_id):
    return lambda id : position_by_id[id]

def getDist(getPos, id1, id2):
    return np.linalg.norm( getPos(id1) - getPos(id2))

def getDistFromPoint(getPos, name, point):
    return np.linalg.norm( getPos(name) - point)

