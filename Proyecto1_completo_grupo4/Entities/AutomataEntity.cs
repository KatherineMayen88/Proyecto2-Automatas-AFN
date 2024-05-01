using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto1_completo_grupo4.Entities
{
    public class AutomataEntity
    {

        private string _estados;
        private String[] _estadoInicial;
        private String[] _estadosFinales;
        private List<TransicionEntity> _transiciones;

        //Constructor vacío
        public AutomataEntity()
        { 
        }

        //Constructor que inicializa las propiedades
        public AutomataEntity(string estados, String[] estadoInicial, String[] estadosFinales, List<TransicionEntity> transiciones)
        {
            _estados = estados;
            _estadoInicial = estadoInicial;
            _estadosFinales = estadosFinales;
            _transiciones = transiciones;
        }

        //En adelante son propiedades
        public string Estados
        {
            get { return _estados; }
            set { _estados = value; }
        }

        public String[] EstadoInicial
        {
            get { return _estadoInicial; }
            set { _estadoInicial = value; }
        }

        public String[] EstadosFinales
        {
            get { return _estadosFinales; }
            set { _estadosFinales = value; }
        }

        public List<TransicionEntity> Transiciones
        {
            get { return _transiciones; }
            set { _transiciones = value; }
        }
    }
}
