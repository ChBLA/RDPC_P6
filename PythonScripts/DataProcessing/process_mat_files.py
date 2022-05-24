import mat73
from utils import get_path_to_data_file

MAT_FILE_NAME = 'yahoo_music_training_test_dataset.mat'


def load_mat_file(file_name):
    file_path = str(get_path_to_data_file(f"mat_files/{file_name}"))
    data_dict = mat73.loadmat(file_path)
    return data_dict


if __name__ == "__main__":
    res = load_mat_file(MAT_FILE_NAME)

