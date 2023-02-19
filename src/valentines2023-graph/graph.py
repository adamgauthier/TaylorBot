import networkx as nx
import matplotlib.pyplot as plt
import csv
import pandas as pd
import os
dir_path = os.path.dirname(os.path.realpath(__file__))
# from networkx.drawing.nx_agraph import graphviz_layout

# Read in the CSV file
with open(dir_path + '/valentines2023-simplified.csv', "r", encoding="utf-8") as f:
    reader = csv.reader(f)
    next(reader)  # skip the first row
    edges = [(row[1], row[0]) for row in reader]

# Create the graph object
G = nx.DiGraph()
G.add_edges_from(edges)

# Ensure that the graph is acyclic
if not nx.is_directed_acyclic_graph(G):
    raise ValueError('Graph contains cycles')

# pos = nx.spring_layout(G)

labels = {}
for node in G.nodes:
    labels[node] = node
nx.set_node_attributes(G, labels, 'label')

df = pd.DataFrame(index=G.nodes(), columns=G.nodes())
for row, data in nx.shortest_path_length(G):
    for col, dist in data.items():
        df.loc[row,col] = dist

df = df.fillna(df.max().max())

# Use this for a tree instead? https://stackoverflow.com/questions/61209974/create-a-tree-structure-from-a-graph
pos = nx.kamada_kawai_layout(G, dist=df.to_dict(), scale=2, center=(0, 1))

# Generate the layout with the first node at the top
# pos = nx.kamada_kawai_layout(G, scale=2, center=(0, 1))


# pos = graphviz_layout(G, prog='dot')

nx.draw(G, pos=pos,
        node_color='lightpink',
        node_size=1500,
        with_labels=True,
        arrows=True,
        margins=0.001)

# pos_higher = {}
# y_off = 0.08  # offset on the y axis

# for k, v in pos.items():
#     pos_higher[k] = (v[0], v[1]+y_off)

# nx.draw(G, pos, with_labels=False, font_weight='bold')
# labels = nx.get_node_attributes(G, 'label')

# for k, v in pos_higher.items():
#     plt.text(v[0],v[1],s=labels[k], bbox=dict(facecolor='red', alpha=0.5),horizontalalignment='center')

# Draw the graph
# nx.draw(G, pos, with_labels=False, font_weight='bold')
# labels = nx.get_node_attributes(G, 'label')
# label_pos = {k: (v[0], v[1]+0.05) for k,v in pos.items()}
# nx.draw_networkx_labels(G, pos=label_pos, labels=labels, font_size=12, font_color='k')

plt.axis('off')
plt.show()
