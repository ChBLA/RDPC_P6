from matplotlib.pyplot import get
import numpy as np
import difflib as search
import loading
import query_util as util
import plot

def getInput():
    return input().split(";")

def printCloseTo(getPos, names, name):
    return printCloseToPoint(getPos, names, getPos(name))

def printCloseToPoint(getPos, names, point):
    closeMovies = util.getClosestToPoint(getPos, names, point, 10)
    for i in range(len(closeMovies)):
        print(f"{i}: {distString(closeMovies[i][0])} | {closeMovies[i][1]}")
    return closeMovies

def printCloseToSegment(getPos, names, name1, name2):
    print(f"On a scale from {name1} to {name2}")
    closeMovies = util.getClosestToSegmentComponents(getPos, names, name1, name2, 10)
    for i in range(len(closeMovies)):
        percentage = f"{int(100 * closeMovies[i][1])}%"
        percentage = percentage if len(percentage) >= 5 else " " * (5 - len(percentage)) + percentage
        print(f"{i}: {percentage} | {closeMovies[i][2]}")
    return [(movie[0], movie[2]) for movie in closeMovies]

def printAddition(getPos, names, name1, name2):
    print(f"The closest to the sum of {name1} and {name2} are:")
    printCloseToPoint(getPos, names, util.addMovies(getPos, name1, name2))

def printAddChange(getPos, names, name1, name2, name3):
    print(f"The closest to the difference from {name1} to {name2} added to {name3} are:")
    printCloseToPoint(getPos, names, util.addMovieDiff(getPos, name1, name2, name3))

def searchMovieNames(targets, names):
    closeMovies = []
    lowerCaseNames = [name.lower() for name in names]
    lowercase_indexed = {name.lower(): name for name in names}

    for t in targets:
        targetName = t.lower().strip()
        suggestions = search.get_close_matches(targetName, lowerCaseNames, n=1, cutoff=1)
            
        if len(suggestions) > 0:
            closeMovies.append(lowercase_indexed[suggestions[0]])
        else:
            suggestions = [name for name in lowerCaseNames if targetName in name]
            if len(suggestions) > 0:
                closeMovies.append(lowercase_indexed[suggestions[0]])
            else:
                print(f"{t} was not found as a movie title")
                return None
    return closeMovies

def closeToIndex(index, getPos, allNames, data):
    if isinstance(data, list):
        if index < len(data) and index >= 0:
            return printCloseTo(getPos, allNames, data[index][1])
        else:
            print(f"{index} is an invalid index")
    else:
        print("No recent movie list")
    return None

def distString(value):
    decimals = 3
    length = decimals + 3
    dist = str(round(value, decimals))
    return dist + "0" * (length - len(dist)) if len(dist) < length else dist

def printDist(getPos, name1, name2):
    print(f"{distString(util.getDist(getPos, name1, name2))} between {name1} and {name2}")
    return None

def printHelp():
    print("------------------------------ HELP ------------------------------")
    print("help                           : lists the available commands")
    print("plot; name1, name2, name3      : plots movies close to the plane through the movies")
    print("dist; name1; name2,            : finds the distance between name1 and name2")
    print("closeto; name1                 : lists the closest movies to name1")
    print("scale; name1; name2            : list movies on a scale from name1 to name2")
    print("addchange; name1; name2; name3 : adds to name3 the vector from name1 to name2")
    print("                                 and lists close movies")
    print("goto; index                    : lists movies close to movie at index in the recent list")
    print("exit/quit                      : closes the program")

def movieQuery():
    small_data_set = {"cloud": "point_clouds/movielens_data_points.json", "titles" : "raw_data/movies.dat"}
    big_data_set = {"cloud": "point_clouds/ml_points_25m.json", "titles" : "raw_data/movies_25m.csv"}

    (position_by_id, id_indexed, name_indexed) = loading.movieQueryData(small_data_set, csv = False)
    getPos = util.getPosition(position_by_id, name_indexed)
    allNames = list(name_indexed.keys())
    running = True
    data = None

    movieCommands = {"plot" : lambda names : plot.plot(getPos, allNames, names[0], names[1], names[2]),
                    "scale": lambda names : printCloseToSegment(getPos, allNames, names[0], names[1]),
                    "addchange": lambda names : printAddChange(getPos, allNames, 
                                                                names[0], names[1], names[2]),
                    "add": lambda names : printAddition(getPos, allNames, names[0], names[1]),
                    "dist": lambda names : printDist(getPos, names[0], names[1]),
                    "closeto": lambda names : printCloseTo(getPos, allNames, names[0]),
                    "help": lambda names : printHelp()}
    otherCommands = {"goto": lambda index : closeToIndex(int(index[0].strip()), getPos, allNames, data)}

    print("Ready!")

    while running:
        inputs = getInput()
        command = inputs[0].lower().strip()
        
        if command == "exit" or command == "quit":
            running = False
        elif command in movieCommands:
            movieNames = searchMovieNames(inputs[1:], allNames)
            if not (movieNames is None):
                data = movieCommands[command](movieNames)
        elif command in otherCommands:
            data = otherCommands[command](inputs[1:])
        else:
            print(f"{command} is not a reckognised command")

    #print(getDist(getPos, "Toy Story (1995)", "Toy Story 2 (1999)"))
    
if __name__ == "__main__":
    movieQuery()