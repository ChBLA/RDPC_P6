import gzip
import json
import csv
import os
import copy
import numpy as np
from pathlib import Path
import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

def get_path_to_data_file(file_name):
    return Path.joinpath(ph.data_dir_path, file_name)

def get_path_to_raw_data_file(file_name):
    return Path.joinpath(ph.data_raw_data_path, file_name)

def get_path_to_pc_file(file_name):
    return Path.joinpath(ph.data_point_cloud_path, file_name)

def load_json(file_name, isPC):
    path = get_path_to_raw_data_file(file_name) if not isPC else get_path_to_pc_file(file_name)
    with open(path, "r") as file:
        return json.loads(file.read())

def getDatData(file_name):
    path = get_path_to_data_file(file_name)
    id_indexed = {}
    with open(path, "r") as f:
        entries = f.read().split("\n")
        for entry in entries:
            if "::" in entry:
                vals_in_entry = entry.split("::")
                id = vals_in_entry[0]
                name = vals_in_entry[1]
                id_indexed[id] = name
        
    return id_indexed

def getCSVData(file_name):
    path = get_path_to_data_file(file_name)
    id_indexed = {}
    with open(path, "r", encoding="utf-8") as csvfile:
        file = csv.DictReader(csvfile)
        for row in file:
            #parsed_row = {unicode(key, 'utf-8'):unicode(value, 'utf-8') for key, value in row.iteritems()}
            id_indexed[row["movieId"]] = row["title"]
    return id_indexed

"""
point_cloud is the name of the file in which the point cloud to be tested is located
test_data is the name of the file for which the test data is located
"""
def pointCloudData(point_cloud, test_data):
    cloud = load_json(point_cloud, True)
    test_ratings = load_json(test_data, False)
    positions = cloud["Positions"]
    ids = cloud["Ids"]

    position_by_id = {}

    for i in range(len(positions)):
        position_by_id[ids[i]] = np.array(positions[i])
        
    return (position_by_id, test_ratings)
