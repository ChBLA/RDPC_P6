from pathlib import Path
import json 

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()


def get_path_to_data_file(file_name):
    return Path.joinpath(ph.data_raw_data_path, file_name)


def saveData(data, path):
    print(f"Writing data to path '{path}'")
    with open(path, 'w') as file:
        json.dump(data, file, indent=2)

def saveCSVData(data, path):
    # open the file in the write mode
    f = open(path, 'w')

    # create the csv writer
    writer = csv.writer(f)

    # write a row to the csv file
    writer.writerow(data)

    # close the file
    f.close()


def read_json_file(path_to_json_file):
    with open(path_to_json_file, 'r') as json_file:
        data = json_file.read()
    return json.loads(data)

