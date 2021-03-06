using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace TreeStructure.Tests
{
    [TestFixture]
    public class NodeRepository_Fixture
    {
        private ISessionFactory _sessionFactory;
        private NodeRepository _repository;
        private Node _root, _child1, _child2, _child1_1, _child1_2, _child1_3, _child2_1, _child2_2;

        [SetUp]
        public void SetupContext()
        {
            var configuration = new Configuration();
            configuration.Configure();
            configuration.AddAssembly(typeof(Equipment).Assembly);
            new SchemaExport(configuration).Execute(false, true, false, false);

            _sessionFactory = configuration.BuildSessionFactory();

            CreateInitialData();

            _repository = new NodeRepository();
        }

        private void CreateInitialData()
        {
            _root = new Node {Name = "root"};
            _child1 = new Node {Name = "child1"};
            _child2 = new Node {Name = "child2"};
            _child1_1 = new Node { Name = "grand child 1-1" };
            _child1_2 = new Node { Name = "grand child 1-2" };
            _child1_3 = new Node { Name = "grand child 1-3" };
            _child2_1 = new Node { Name = "grand child 2-1" };
            _child2_2 = new Node { Name = "grand child 2-2" };

            _root.AddChild(_child1);
            _root.AddChild(_child2);
            _child1.AddChild(_child1_1);
            _child1.AddChild(_child1_2);
            _child1.AddChild(_child1_3);
            _child2.AddChild(_child2_1);
            _child2.AddChild(_child2_2);

            using (var session = _sessionFactory.OpenSession())
            {
                session.Save(_root);
                session.Flush();
            }
        }

        [Test]
        public void Can_load_aggregate_by_id()
        {
            var id = _root.Id;
            var node = _repository.GetAggregateById(id);
            Assert.IsNotNull(node);
            Assert.IsTrue(NHibernateUtil.IsInitialized(node.Parent));
            Assert.IsTrue(NHibernateUtil.IsInitialized(node.Children));
            Assert.IsTrue(NHibernateUtil.IsInitialized(node.Ancestors));
            Assert.IsTrue(NHibernateUtil.IsInitialized(node.Descendants));
            Assert.IsNull(node.Parent);
            Assert.AreEqual(2, node.Children.Count);
            Assert.AreEqual(0, node.Ancestors.Count);
            Assert.AreEqual(7, node.Descendants.Count);
        }

        [Test]
        public void Can_get_ancestors()
        {
            var id = _child1_3.Id;
            var node = _repository.GetAggregateById(id);
        }
    }
}