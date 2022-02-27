import pandas
#import numpy as np

#path 
csv_table = pandas.read_csv("world_coordinate_inter_eth.csv")

#csv_table = np.array([1,2,3])
transposed = csv_table.T
#transposed = transposed.dropna()
#transposed = transposed[1:]
transposed.to_csv("world_coordinate_inter_eth_T.csv")

