import math
import numpy as np
from pymoo.model.problem import Problem

class MMF4(Problem):

    def __init__(self, **kwargs):
        super().__init__(n_var=2, n_obj=2, n_constr=0, xl=np.array([-1, 0]), xu=np.array([1, 2]), **kwargs)

    def _calc_pareto_front(self, n_pareto_points = 100):
        pareto_points = np.linspace(0, 1, n_pareto_points)
        pareto_front = np.array([[f1, 1 - math.pow(f1, 2)] for f1 in pareto_points])
        return pareto_front

    def f1(self, x_1):
        return abs(x_1)
    
    def f2(self, x_1, x_2):
        if (x_2 >= 0 and x_2 < 1):
            return 1 - math.pow(x_1, 2) + 2 * math.pow(x_2 - math.sin(math.pi * abs(x_1)), 2)
        elif (x_2 >= 1 and x_2 <= 2):
            return 1 - math.pow(x_1, 2) + 2 * math.pow(x_2 - 1 - math.sin(math.pi * abs(x_1)), 2)

    def _evaluate(self, x, out, *args, **kwargs):
        out["F"] = [[ self.f1(vector[0]), self.f2(vector[0], vector[1]) ] for vector in x]