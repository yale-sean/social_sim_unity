with open('world_coordinate_inter_hotel.csv', 'r') as f:
    frameids = f.readline().strip().split(',')
    ids = f.readline().strip().split(',')
    xs = f.readline().strip().split(',')
    ys = f.readline().strip().split(',')

with open(f"hotel.csv", 'w') as f:
    w = csv.writer(f)
    for i, frameid in enumerate(frameids):
        w.writerow([frameid, ids[i], xs[i], ys[i]])
