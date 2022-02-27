import numpy as np
import pandas as pd


pathFile = "world_coordinate_inter_hotel.csv"


df = pd.read_csv(pathFile, header = None)
df_data = df.T
df_data[0] = df_data[0].map(lambda x: '%d' % x)
df_data[1] = df_data[1].map(lambda x: '%d' % x)
df_data[2] = df_data[2].map(lambda x: '%1.4f' % x)
df_data[3] = df_data[3].map(lambda x: '%1.4f' % x)

outputName = "hotelTest.csv"
df_data.to_csv(outputName, header = None, index=False)
