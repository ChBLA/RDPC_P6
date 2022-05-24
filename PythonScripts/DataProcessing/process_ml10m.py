import json
from utils import read_json_file, get_path_to_data_file, saveData
from constants import USER_ID_KEY, MOVIE_ID_KEY

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

from pathlib import Path
from extract_meta_data import extract_meta_data
from processing import process_complete_dat

if __name__ == "__main__":
    process_complete_dat("ml10m_ratings.dat", "ml10m_complete.json")
    print(f"Ratings succesfully converted.\nStarting meta data extraction")

    json_file_path = get_path_to_data_file("ml10m_complete.json")
    meta_data = extract_meta_data(json_file_path, USER_ID_KEY, MOVIE_ID_KEY)

    save_path = Path.joinpath(ph.data_dir_path, "ml10m_complete_points.json")
    print(f"Writing extracted metadata to save_path: '{save_path}'")
    saveData(meta_data, save_path)
    print("Done with ML10M data processing!")