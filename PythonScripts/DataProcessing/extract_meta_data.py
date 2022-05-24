import json
from utils import read_json_file, get_path_to_data_file, saveData
from constants import USER_ID_KEY, MOVIE_ID_KEY

import setup_paths
from PathHandler import PathHandler
ph = PathHandler()
from pathlib import Path

def extract_meta_data(path_to_file, user_id_key, item_id_key):
    json_obj = read_json_file(path_to_file)
    meta_data = extract_all_unique_entries(json_obj, user_id_key, item_id_key)
    return meta_data

def extract_all_unique_entries(json_obj, user_entry_name, item_entry_name):
    unique_ids = []
    for rating in json_obj:
        user_id = rating[user_entry_name]
        item_id = rating[item_entry_name]
        #print(f"Processing rating with: user_id: '{user_id}', item_id: '{item_id}'")
        assert user_id != item_id
        if user_id not in unique_ids:
            #print(f"   Adding user_id: '{user_id}'")
            unique_ids.append(user_id)
        if item_id not in unique_ids:
            #print(f"   Adding  item_id: '{item_id}'")
            unique_ids.append(item_id)
    return unique_ids

def main():
    json_file_path = get_path_to_data_file("ml10m_complete.json")
    meta_data = extract_meta_data(json_file_path, USER_ID_KEY, MOVIE_ID_KEY)

    save_path = Path.joinpath(ph.data_dir_path, "ml10m_complete_points.json")
    print(f"Writing extracted metadata to save_path: '{save_path}'")
    saveData(meta_data, save_path)

if __name__ == "__main__":
    #main()
    ...