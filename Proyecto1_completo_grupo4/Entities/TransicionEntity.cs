using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto1_completo_grupo4.Entities
{
    public class TransicionEntity
    {

        private string _estadoOrigen;
        private string _simbolo;
        private string _estadoDestino;

        //Constructor vacío
        public TransicionEntity()
        {
        }

        //Constructor que inicializa las propiedades
        public TransicionEntity(string estadoOrigen, string simbolo, string estadoDestino)
        {
            _estadoOrigen = estadoOrigen;
            _simbolo = simbolo;
            _estadoDestino = estadoDestino;
        }

        //En adelante son propiedades
        public string EstadoOrigen
        {
            get { return _estadoOrigen; }
            set { _estadoOrigen = value; }
        }

        public string Simbolo
        {
            get { return _simbolo; }
            set { _simbolo = value; }
        }

        public string EstadoDestino
        {
            get { return _estadoDestino; }
            set { _estadoDestino = value; }
        }

    }
}
