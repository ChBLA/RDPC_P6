import numpy as np
from scipy.sparse import csc_matrix
import numpy as np
import h5py
from pathlib import Path

from utils import get_path_to_data_file, saveData
from constants import JSON_EXT

from PathHandler import PathHandler
ph = PathHandler()

def load_matlab_file(path_file, name_field):
    ''' From https://github.com/usydnlp/Glocal_K '''

    db = h5py.File(path_file, 'r')
    ds = db[name_field]

    try:
        if 'ir' in ds.keys():
            data = np.asarray(ds['data'])
            ir   = np.asarray(ds['ir'])
            jc   = np.asarray(ds['jc'])
            out  = csc_matrix((data, ir, jc)).astype(np.float32)
    except AttributeError:
        print("AttributeError")
        out = np.asarray(ds).astype(np.float32).T

    db.close()

    return out

def load_data_monti(path):
    ''' From https://github.com/usydnlp/Glocal_K '''

    M = load_matlab_file(path, 'M')
    Otraining = load_matlab_file(path, 'Otraining') * M
    Otest = load_matlab_file(path, 'Otest') * M

    n_u = M.shape[0]  # num of users
    n_m = M.shape[1]  # num of movies
    n_train = Otraining[np.where(Otraining)].size  # num of training ratings
    n_test = Otest[np.where(Otest)].size  # num of test ratings

    train_r = Otraining.T
    test_r = Otest.T

    train_m = np.greater(train_r, 1e-12).astype('float32')  # masks indicating non-zero entries
    test_m = np.greater(test_r, 1e-12).astype('float32')

    print('data matrix loaded')
    print('num of users: {}'.format(n_u))
    print('num of movies: {}'.format(n_m))
    print('num of training ratings: {}'.format(n_train))
    print('num of test ratings: {}'.format(n_test))

    return n_m, n_u, train_r, train_m, test_r, test_m, n_train, n_test, n_u, n_m

def get_douban_file_path(prepend_path, file_name):
    dir_path = Path(__file__).parent
    douban_dir_path = Path.joinpath(dir_path, prepend_path)
    file_path = str(Path.joinpath(douban_dir_path, file_name))
    return file_path

def process_douban_dataset(input_file_name, output_file_name, output_metafile_name):
    file_path = str(get_path_to_data_file(f"mat_files/{input_file_name}"))
    (n_m, n_u, train_r, train_m, test_r, test_m, n_train, n_test, n_u, n_m) = load_data_monti(file_path)
   
    print_distribution(n_train, n_test)
    # train_m is a masked version of train_r: train_m
    # inspect_train_m(train_m)

    n_u_m_train = process_partition(train_r, output_file_name, output_metafile_name, "train", n_train)
    n_u_m_test = process_partition(test_r, output_file_name, output_metafile_name, "test", n_test)
 
    contains_all = lambda u_m_count : u_m_count == n_u + n_m
    check_unique_data_points(contains_all, n_u_m_train, n_u_m_test)

def print_distribution(n_train, n_test):
    total = n_train + n_test

    pct = lambda n_data : ( (n_data) / total ) * 100

    print("Dataset partitioning:")
    print(f"  Training: {round(pct(n_train))}%")
    print(f"  Test: {round(pct(n_test))}%")

def process_partition(data, output_file_name, output_metafile_name, flag_str, expected_rating_count):
    print(f"Processing {flag_str} partition.")
    data = data.tolist()
    len_ratings = produce_ratings_file(data, get_file_name(output_file_name, flag_str, JSON_EXT)) 
    assert len_ratings == expected_rating_count

    count = produce_metadata_file(data, get_file_name(output_metafile_name, flag_str, JSON_EXT))
    return count

def get_file_name(name, flag_str, ext):
    return name + "_" + flag_str + ext

def check_unique_data_points(contains_all, train_u_m_count, test_u_m_count):
    assert contains_all(train_u_m_count)
    assert contains_all(test_u_m_count)

def inspect_train_m(train_m):
    train_m = train_m.tolist()
    for r in range(len(train_m)):
        row_u = train_m[r]
        for c in range(len(row_u)):
            if row_u[c] != 0.0:
                rating = row_u[c]
                assert rating % 1 == 0.0
                # print(f"entry[{r}][{c}]: {rating}")

def add_entry(container, user_id, rating, movie_id):
    content = { "user_id": get_user_str(user_id), 
                "movie_id": str(movie_id),
                "rating": int(rating) }
    container.append(content)

def get_user_str(user_id):
    return "u" + str(user_id)

def produce_ratings_file(user_item_matrix, output_file_name):
    ratings = []
    for r in range(len(user_item_matrix)):
        row_u = user_item_matrix[r]
        for c in range(len(row_u)):
            if row_u[c] != 0.0:
                rating = row_u[c]
                assert rating % 1 == 0.0
                add_entry(ratings, r, rating, c)
    
    output_file_path = Path.joinpath(ph.data_raw_data_path, output_file_name)
    saveData(ratings, output_file_path)

    return len(ratings)

def produce_metadata_file(user_item_matrix, output_file_name):
    userCount = 0
    movieCount = 0
    points = []
    for user_id in range(len(user_item_matrix)):
        userCount += 1
        points.append(get_user_str(user_id))

    for movie_id in range(len(user_item_matrix[0])):
        movieCount += 1
        points.append(str(movie_id))

    output_file_path = Path.joinpath(ph.data_raw_data_path, output_file_name)
    saveData(points, output_file_path)

    return userCount + movieCount


if __name__ == "__main__":
    process_douban_dataset("douban_monti_dataset.mat", "douban_ratings", "douban_meta")