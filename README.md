# Gradient Descent for Relative Point Clouds

## Overview

| Folder                                | Description                                                     |
|---------------------------------------|-----------------------------------------------------------------|
| [ PythonScripts ]( ./PythonScripts/ ) | Mainly scripts for data processing and vizualization.           |
| [ P6 ]( ./P6 )                        | Implementation of the algorithm, experiments, etc.              |
| [Data](./Data)                        | Data for training, training history, metadata and point clouds. |


## Files
To run the experiments for this project, the movielens datasets (MovieLens 1M Dataset and MovieLens 10M Dataset) need to be downloaded.
The datasets can be found at: ML1M = https://grouplens.org/datasets/movielens/1m/, ML10M = https://grouplens.org/datasets/movielens/10m/.
Make sure to move the "ratings.dat" file from each dataset to the [Data/raw_data](Data/raw_data) using names "ml1m_ratings.dat" and "ml10m_ratings.dat" for ML1M and ML10M, respectively. 
In order to use these datasets, the following commands must be run from the [PythonScripts](PythonScripts) folder:
```
python process_ml1m.py
python process_ml10m.py
```
Both commands may be time-consuming, especially so for the second. 

## Experiments

Experiment configurations can be found in [P6/Settings/Experiments](P6/Settings/Experiments).

Each folder in [P6/Settings/Experiments](P6/Settings/Experiments) correspond to a single experiment and such a folder contains two files `AppConfig.json` and `OptimizerConfig.json` specifying the setup for the experiment.

### Running experiments

The following procedure can be followed to run `SomeExperiment` using Docker:

```zsh 
# build image (assuming that current working directory is the root of the codebase)
docker build -t p6_experiments:0.1 .

# start the container
# this will start executing the experiment represented by folder SomeExperiment 
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments SomeExperiment
```

Enabling sharing of data directory between host machine and docker container:
- The command `-v "$(pwd)"/Data:/reldist/Data` assumes that the current working directory contains the Data folder (the folder described in [Overview](#overview)).
- Alternatively, the command `-v src:/reldist/Data` can be used where `src` should be a path to the Data folder.

Note:
- The container can be inspected by executing the following command: 

```bash
docker run --entrypoint "/bin/bash" -it --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1
```
### Tuning experiments
RTD tuning experiments x ∈ {DoubanRTDConstantTuningExperiment, DoubanRTDPowerTuningExperiment, ML1MRTDConstantTuningExperiment, ML1MRTDPowerTuningExperiment} run:
```
docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments x
```

Decay function tuning for ML1M and Douban for x ∈ {DoubanTuneDecayConstantExperiment, DoubanTuneDecayFactorExperiment, DoubanTuneDecayBaseExperiment, ML1MTuneDecayConstantExperiment, ML1MTuneDecayFactorExperiment, ML1MTuneDecayBaseExperiment} and diffusion degree # run:
```
docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments x #
```

Tune dimensions for ML1M and Douban for x ∈ {DoubanDimensionTuningExperiment, ML1MDimensionTuningExperiment} run:
```
docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments x
```

Tune optimisers for ML1M and Douban for x ∈ {DoubanOptimiserTuningExperiment, ML1MOptimiserTuningExperiment} run:
```
docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments x
```

### Benchmark experiments
To perform benchmarking on the MovieLens 1M (ML1M) and Douban datasets for the RDPC model run:
```
docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments ML1MVanillaBenchmark

docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments DoubanVanillaBenchmark
```

To perform benchmark on the MovieLens 1M (ML1M) and Douban datasets for the RDPC-D with diffusion degree "#" run:
```
docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments ML1MDiffusionBenchmark #

docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments DoubanDiffusionBenchmark #
```

To perform scalability experiment on MovieLens 10M (ML10M) for diffusion degree # run:
```
docker build -t p6_experiments:0.1 .
docker run --rm -v "$(pwd)"/Data:/reldist/Data p6_experiments:0.1 Experiments ML10MTrueDiffusionExperiment #
```

