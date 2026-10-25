import unittest

import numpy as np

from sentiment import dominant_emotion, label_for
from train import LogisticModel, roc_auc


class LogisticModelTests(unittest.TestCase):
    def test_learns_a_separable_pattern(self):
        rng = np.random.default_rng(0)
        X = rng.normal(size=(400, 2))
        y = (X[:, 0] + 0.2 * rng.normal(size=400) > 0).astype(int)

        model = LogisticModel().fit(X, y)
        probability = model.predict_proba(X)

        self.assertGreater(roc_auc(y, probability), 0.95)
        self.assertTrue(((probability >= 0) & (probability <= 1)).all())


class RocAucTests(unittest.TestCase):
    def test_perfect_and_reversed_ranking(self):
        self.assertEqual(roc_auc([0, 0, 1, 1], [0.1, 0.2, 0.8, 0.9]), 1.0)
        self.assertEqual(roc_auc([0, 0, 1, 1], [0.9, 0.8, 0.2, 0.1]), 0.0)

    def test_ties_give_one_half(self):
        self.assertEqual(roc_auc([0, 1, 0, 1], [0.5, 0.5, 0.5, 0.5]), 0.5)

    def test_single_class_returns_one_half(self):
        self.assertEqual(roc_auc([1, 1, 1], [0.1, 0.2, 0.3]), 0.5)


class SentimentHelpersTests(unittest.TestCase):
    def test_labels(self):
        self.assertEqual(label_for(0.5), "positive")
        self.assertEqual(label_for(-0.5), "negative")
        self.assertEqual(label_for(0.0), "neutral")

    def test_dominant_emotion(self):
        self.assertEqual(dominant_emotion(["Shares surge as profit beats estimates"]), "optimism")
        self.assertEqual(dominant_emotion(["SEBI probe into fraud"]), "anger")
        self.assertEqual(dominant_emotion(["Company holds annual meeting"]), "neutral")


if __name__ == "__main__":
    unittest.main()
