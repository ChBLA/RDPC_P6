import pytest
from constants import USER_ID_KEY, MOVIE_ID_KEY
from extract_meta_data import extract_all_unique_entries

class TestExtracter():
    def test__extract_all_unique_entries__base_case(self):
        json_input = [
            {USER_ID_KEY: "u1", MOVIE_ID_KEY: "1"},
            {USER_ID_KEY: "u2", MOVIE_ID_KEY: "2"},
            {USER_ID_KEY: "u3", MOVIE_ID_KEY: "3"},
        ]
        expected_output = [ "u1", "1", "u2", "2", "u3", "3" ]
        
        actual_output = extract_all_unique_entries(json_input, USER_ID_KEY, MOVIE_ID_KEY)
        
        assert expected_output == actual_output

    def test__extract_all_unique_entries__interleave(self):
        json_input = [
            {USER_ID_KEY: "u1", MOVIE_ID_KEY: "1"},
            {USER_ID_KEY: "u1", MOVIE_ID_KEY: "2"},
            {USER_ID_KEY: "u2", MOVIE_ID_KEY: "1"},
            {USER_ID_KEY: "u3", MOVIE_ID_KEY: "1"},
            {USER_ID_KEY: "u4", MOVIE_ID_KEY: "3"},
        ]
        expected_output = [ "u1", "1", "2", "u2", "u3", "u4", "3" ]
        
        actual_output = extract_all_unique_entries(json_input, USER_ID_KEY, MOVIE_ID_KEY)
        
        assert expected_output == actual_output
