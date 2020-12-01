from pymoo.algorithms.nsga2 import NSGA2
from pymoo.factory import get_crossover, get_mutation

def Get_Algorithm_Instance(pop_size = 100, crossover_probability = 0.9, mutation_probability = None):
    crossover = get_crossover("real_sbx", prob = crossover_probability, eta = 15)
    mutation = get_mutation("real_pm", prob = mutation_probability, eta = 20)
    return NSGA2(pop_size = pop_size, crossover = crossover, mutation = mutation)