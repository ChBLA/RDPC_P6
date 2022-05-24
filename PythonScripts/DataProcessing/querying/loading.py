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

def load_json(file_name):
    path = get_path_to_data_file(file_name)
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

def movieQueryData(data_set, csv):
    
    cloud = load_json(data_set["cloud"])
    positions = cloud["Positions"]
    ids = cloud["Ids"]

    position_by_id = {}

    for i in range(len(positions)):
        if ids[i][0] != "u":
            position_by_id[ids[i]] = np.array(positions[i])

    id_indexed = getCSVData(data_set["titles"]) if csv else getDatData(data_set["titles"])
    
    return (position_by_id, 
            {id: id_indexed[id]  for id in position_by_id.keys()}, 
            {id_indexed[id]: id  for id in position_by_id.keys()})