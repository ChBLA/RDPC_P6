from pathlib import Path
import json

class PathHandler():
    def __init__(self):
        self.cwd_path = Path(__file__).resolve()
        self.code_dir_path = self.cwd_path.parent.parent
        self.data_dir_path = Path.joinpath(self.code_dir_path, "Data")
        self.data_run_hist_path = Path.joinpath(self.data_dir_path, "run_history")
        self.data_point_cloud_path = Path.joinpath(self.data_dir_path, "point_clouds")
        self.data_raw_data_path = Path.joinpath(self.data_dir_path, "raw_data")

if __name__ == "__main__":
    ph = PathHandler()
    print(f"cwd_path: { str(ph.cwd_path) }")
    print(f"code_dir_path: { str(ph.code_dir_path) }")
    print(f"data_dir_path: { str(ph.data_dir_path)}")
    print(f"data_run_hist_path: { str(ph.data_run_hist_path) }")
    print(f"data_point_cloud_path: { str(ph.data_point_cloud_path) }")
    print(f"data_raw_data_path: { str(ph.data_raw_data_path) }")
