from matplotlib.pyplot import get
import numpy as np
from scipy import stats
import numpy as np
import loading
import util
import math
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('-q', dest="quiet", action="store_true", help="indicates whether to print only final results")
parser.add_argument('-m', dest="multipleFiles", action="store_true", help="using multiple files and measure confidence")
parser.add_argument('--rtd', dest="rtd", type=str, default="douban", help='rtd function; either douban or movielens')
parser.add_argument('--files', dest='files', type=str, nargs='+', help='names of all files to use')
args = parser.parse_args()



def mean_confidence_interval(data, confidence=0.95):
    """Stolen from stack-overflow"""
    a = np.array(data, dtype=np.float32)
    n = len(a)
    m, se = np.mean(a), stats.sem(a)
    h = se * stats.t.ppf((1 + confidence) / 2., n-1)

    return m, h

def get_dtr_func():
    if (args.rtd == "douban"):
        return lambda dist : 6.0 - math.pow(dist, 1.0/1.3)
    elif (args.rtd == "movielens"):
        return lambda dist : 6.4 - math.pow(dist, 1.0/1.5)
    else:
        return lambda dist : 6.0 - dist

def get_test_rmse(point_cloud, test_ratings):
    test_se = 0.0
    pos_func = util.getPosition(point_cloud)
    counter = 0
    missed = 0
    dtr_func = get_dtr_func()

    for rating in test_ratings:
        actual_dist = 3.0
        if (rating["user_id"] in point_cloud and rating["movie_id"] in point_cloud):
            actual_dist = dtr_func(util.getDist(pos_func, rating["user_id"], rating["movie_id"]))
            ...
        else:
            missed += 1

        test_se += (actual_dist - float(rating["rating"])) * (actual_dist - float(rating["rating"]))
        counter += 1
    
    test_mse = test_se / counter
    test_rmse = math.sqrt(test_mse)
    if not args.quiet:
        print(f"Missed {missed} out of {counter}")
        print(f"Test_mse: {test_mse}, rmse: {test_rmse}")
    return test_rmse

if __name__ == "__main__":
    if (args.multipleFiles):
        test_errors = []
        for file in args.files:
            test_file = "douban_ratings_test.json" if args.rtd == "douban" else "movielens_test_ratings.json"
            point_cloud, test_ratings = loading.pointCloudData(file, test_file)
            test_rmse = get_test_rmse(point_cloud, test_ratings)
            print(test_rmse)
            test_errors.append(test_rmse)
        m, h = mean_confidence_interval(test_errors)
        print(f"m:{m}\nh:{h}")
    else:
        if (args.files is None):
            point_cloud, test_ratings = loading.pointCloudData("movielens_data_points.json", "douban_ratings_test.json")
            print(get_test_rmse(point_cloud, test_ratings))
        else:
            point_cloud, test_ratings = loading.pointCloudData(args.files[0], "douban_ratings_test.json")
            print(get_test_rmse(point_cloud, test_ratings))

