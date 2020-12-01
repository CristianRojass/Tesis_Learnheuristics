from pymoo.algorithms.nsga3 import NSGA3
from pymoo.factory import get_crossover, get_mutation

def Get_Algorithm_Instance(reference_directions, pop_size = None, crossover_probability = 1.0, mutation_probability = None):
    crossover = get_crossover("real_sbx", prob = crossover_probability, eta = 30)
    mutation = get_mutation("real_pm", prob = mutation_probability, eta = 20)
    return NSGA3(ref_dirs = reference_directions, pop_size = pop_size, crossover = crossover, mutation = mutation)