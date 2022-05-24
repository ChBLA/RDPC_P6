import json
from utils import read_json_file, get_path_to_data_file, saveData
from constants import USER_ID_KEY, MOVIE_ID_KEY

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()

from pathlib import Path
from processing import movielens_test_split
from processing import process_complete_dat

if __name__ == "__main__":
    process_complete_dat("ml1m_ratings.dat", "ml1m_complete.json")
    print(f"Ratings succesfully converted.\nStarting test split generation")

    movielens_test_split("ml1m_complete.json", 0.1)
    print("Done with ML1M data processing!")