const express = require('express');
const cors = require('cors');
const { graphqlHTTP } = require('express-graphql');
const { buildSchema } = require('graphql');
const sqlite3 = require('sqlite3').verbose();
const path = require('path');

const dbPath = path.resolve(__dirname, '../central_rest_laravel/database/database.sqlite');
const db = new sqlite3.Database(dbPath);

const schema = buildSchema(`
  type Multa {
    id: Int!
    codigo: String!
    ci: String!
    nombre_conductor: String
    fecha: String!
    tipo_infraccion: String!
    lugar: String!
    monto: Float!
    estado: String!
  }

  type Query {
    multa(codigo: String!): Multa
    multasPorCi(ci: String!): [Multa!]!
    multas: [Multa!]!
  }
`);

const root = {
  multa: ({ codigo }) => {
    return new Promise((resolve, reject) => {
      db.get('SELECT * FROM multas WHERE codigo = ?', [codigo.trim()], (err, row) => {
        if (err) return reject(new Error('Error consultando la base de datos: ' + err.message));
        resolve(row || null);
      });
    });
  },
  multasPorCi: ({ ci }) => {
    return new Promise((resolve, reject) => {
      db.all('SELECT * FROM multas WHERE ci = ? ORDER BY id DESC', [ci.trim()], (err, rows) => {
        if (err) return reject(new Error('Error consultando multas por CI: ' + err.message));
        resolve(rows || []);
      });
    });
  },
  multas: () => {
    return new Promise((resolve, reject) => {
      db.all('SELECT * FROM multas ORDER BY id DESC', [], (err, rows) => {
        if (err) return reject(new Error('Error listando multas: ' + err.message));
        resolve(rows || []);
      });
    });
  },
};

const app = express();
app.use(cors());
app.use('/graphql', graphqlHTTP({
  schema,
  rootValue: root,
  graphiql: true,
}));

const PORT = 4000;
app.listen(PORT, () => {
  console.log(`Servidor GraphQL (Central de Registro) en http://localhost:${PORT}/graphql`);
  console.log(`BD SQLite vinculada: ${dbPath}`);
});
