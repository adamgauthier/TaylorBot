import networkx as nx
import matplotlib.pyplot as plt
import csv
import pandas as pd
import os
dir_path = os.path.dirname(os.path.realpath(__file__))

# Read in the CSV file
with open(dir_path + '/valentines2023.csv', "r", encoding="utf-8") as f:
    reader = csv.reader(f)
    next(reader)  # skip the first row
    edges = [(row[1], row[0]) for row in reader]

# Create the graph object
G = nx.DiGraph()
G.add_edges_from(edges)

# Ensure that the graph is acyclic
if not nx.is_directed_acyclic_graph(G):
    raise ValueError('Graph contains cycles')

labels = {}
for node in G.nodes:
    labels[node] = node
nx.set_node_attributes(G, labels, 'label')

df = pd.DataFrame(index=G.nodes(), columns=G.nodes())
for row, data in nx.shortest_path_length(G):
    for col, dist in data.items():
        df.loc[row,col] = dist

df = df.fillna(df.max().max())

pos = nx.kamada_kawai_layout(G, dist=df.to_dict(), scale=2, center=(0, 1))

nx.draw(G, pos=pos,
        node_color='lightpink',
        node_size=1500,
        with_labels=True,
        arrows=True,
        margins=0.001)

plt.axis('off')
plt.show()
