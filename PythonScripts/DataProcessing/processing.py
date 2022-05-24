import gzip
import json
import csv
import os
import copy
from matplotlib.font_manager import json_load
import random

def get_path_to_data_file(file_name):
    return Path.joinpath(ph.data_dir_path, file_name)
def get_path_to_raw_data_file(file_name):
    return Path.joinpath(ph.data_raw_data_path, file_name)
from utils import get_path_to_data_file
from utils import saveData, saveCSVData

def getData(path):
    data = []
    with gzip.open(path, "r") as f:
        text = f.read().decode('utf-8')
        text = "[" + text.replace("}\n{", "},\n{") + "]"
        data = json.loads(text)
    return data

def getCSVData(path):
    data = []
    with open(path, "r") as csvfile:
        file = csv.DictReader(csvfile)
        for row in file:
            data.append({"user_id": "u" + row["userId"], "movie_id": row["movieId"], "rating": row["rating"]})
    return data

def getDatData(file_name):
    formatted_entries = []
    with open(get_path_to_data_file(file_name), "r") as f:
        entries = f.read().split("\n")
        for entry in entries:
            if len(entry) < 3:
                continue;
            vals_in_entry = entry.split("::")
            formatted_entries.append({"user_id": "u" + vals_in_entry[0], "movie_id": vals_in_entry[1], "rating": vals_in_entry[2]})
        
    return formatted_entries

def saveData(data, path):
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

def process_complete_dat(dataFile, saveFile):
    data = getDatData(dataFile)
    print(len(data))
    saveData(data, get_path_to_data_file(saveFile))

def process_amazon_dm():
    item_key = "1387670400"
    user_key = "A1ZCPG3D3HGRSS"
    rating_key = "5.0"
    data = [{"user_id": user_key, "movie_id": item_key, "rating": rating_key}]
    with open(get_path_to_data_file("raw_data/amazon_digital_music.csv"), "r") as csvfile:
        file = csv.DictReader(csvfile)
        
        for row in file:
            data.append({"user_id": row[user_key], "movie_id": row[item_key], "rating": row[rating_key]})

        
    saveData(data, get_path_to_data_file("raw_data/amazon_digital_music.json"))

    ...

def process_goodread():
    data = getData("Data/poetry.json.gz")
    print("loaded!")

    compressed_data = []
    num_of_connections = {}
    for dict in data:
        if dict["is_read"]:
            update_connection_count(num_of_connections, dict, "user_id")       
            update_connection_count(num_of_connections, dict, "book_id")        

            compressed_data.append({"user_id": dict["user_id"], 
                                    "book_id": dict["book_id"],
                                    "rating": dict["rating"]})
    print("compressed")

    flag = True

    while(flag):
        length = len(compressed_data)
        print(f"Amount: {length}")
        kept_data = []

        for dict in compressed_data:
            if num_of_connections[dict["user_id"]] >= 3 and num_of_connections[dict["book_id"]] >= 3:
                kept_data.append(dict)
            else:
                num_of_connections[dict["user_id"]] -= 1
                num_of_connections[dict["book_id"]] -= 1

        compressed_data = kept_data

        flag = len(compressed_data) < length

    saveData(compressed_data, "Data/poetry_ratings_v2.json")

def process_movielens(data):
    num_of_connections = {}
    for dict in data:
        update_connection_count(num_of_connections, dict, "user_id")       
        update_connection_count(num_of_connections, dict, "movie_id")        

    print("compressed")

    flag = True
    init_length = len(data)
    while(flag):
        length = len(data)
        print(f"Amount: {length}")
        kept_data = []

        for dict in data:
            if num_of_connections[dict["user_id"]] >= 3 and num_of_connections[dict["movie_id"]] >= 3:
                kept_data.append(dict)
            else:
                num_of_connections[dict["user_id"]] -= 1
                num_of_connections[dict["movie_id"]] -= 1

        data = kept_data

        flag = len(data) < length

    points = set()
    for dict in data:
        if dict["user_id"] not in points:
            points.add(dict["user_id"])
        if dict["movie_id"] not in points:
            points.add(dict["movie_id"])

    print(f"Found {len(points)} points")

    saveData(list(points), get_path_to_data_file("raw_data/ml_1_points.json"))

    print(f"Removed: {init_length - len(data)}")
    #saveData(data, get_path_to_data_file("raw_data/ml_25_compressed_new.json"))

def get_poorly_connected(num_of_connections, minimum_connections):
    return [id for id in num_of_connections.keys() if num_of_connections[id] < minimum_connections]

def update_connection_count(counts, connection, id):
    if connection[id] in counts.keys():
        counts[connection[id]] += 1
    else :
        counts[connection[id]] = 1

def load_json(file_name):
    path = get_path_to_data_file(file_name)
    with open(path, "r") as file:
        return json.loads(file.read())

def movielens_test_split(file_name, split=0.1):
    ratings = load_json(get_path_to_raw_data_file(file_name))

    test_ratings = []
    training_ratings = []

    for rating in ratings:
        if random.random() < split:
            test_ratings.append(rating)
        else:
            training_ratings.append(rating)

    print(str(len(test_ratings) / len(ratings) * 100)  + "%")
    saveData(test_ratings, get_path_to_raw_data_file("movielens_test_ratings.json"))
    saveData(training_ratings, get_path_to_raw_data_file("movielens_training_ratings.json"))

    ...

def main():
    process_complete_dat("ml10m_ratings.dat", "ml10m_complete.json")
    #movielens_test_split("raw_data/movielens_complete.json")
    #process_amazon_dm()
    #path = get_path_to_data_file("raw_data/ratings.dat")
    #process_movielens(getDatData(path))
    #process_movielens(getCSVData(path))
    #process_movielens()
    #fileDir = os.path.dirname(os.path.realpath('__file__'))
    #saveData(getDatData("Data\\ratings.dat"), os.path.join(fileDir, "DataPreProcessing", "Data\\movielens_ratings.json"))

if __name__ == "__main__":
    main()
