import sys
#print( " sys.path: {}".format(sys.path))

import os
path_env_var = os.environ.get("PYTHONPATH")
#print( " path_env_var: {}".format(path_env_var))

from pathlib import Path
sys.path.append(str(Path(__file__).resolve().parent.parent.parent))

#print( " path_env_var: {}".format(path_env_var))
#print( " sys.path: {}".format(sys.path))
