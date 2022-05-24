import numpy as np
import math

def getPosition(position_by_id, name_indexed):
    return lambda name : position_by_id[name_indexed[name]]

def getDist(getPos, name1, name2):
    return np.linalg.norm( getPos(name1) - getPos(name2))

def getDistFromPoint(getPos, name, point):
    return np.linalg.norm( getPos(name) - point)

def addMovies(getPos, name1, name2):
    return getPos(name1) + getPos(name2)

def addMovieDiff(getPos, name1, name2, name3):
    return getPos(name2) - getPos(name1) + getPos(name3)

def getComponentsToSegment(getPos, name1, name2, name3):
    o = getPos(name1)
    v = getPos(name2) - o
    u = getPos(name3) - o
    v_norm_squared = sum(v**2) 
    proj_of_u_on_v_proportion = (np.dot(u, v)/v_norm_squared)
    anti_proj_of_u_on_v = u - proj_of_u_on_v_proportion * v

    return (np.linalg.norm(anti_proj_of_u_on_v), proj_of_u_on_v_proportion)

def getClosestToPoint(getPos, names, point, count):
    distances = []

    for other in names:
        dist = getDistFromPoint(getPos, other, point)
        distances.append((dist, other))

    distances = sorted(distances, key = lambda tuple : tuple[0])[:count]
    return distances

def getClosestToSegmentComponents(getPos, names, name1, name2, count):
    distances = []

    for other in names:
        dist, scale = getComponentsToSegment(getPos, name1, name2, other)
        distances.append((dist, scale, other))

    distances = sorted(distances, key = lambda tuple : tuple[0])[:count]
    return sorted(distances, key = lambda tuple : tuple[1])