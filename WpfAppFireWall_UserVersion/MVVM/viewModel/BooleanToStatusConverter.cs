using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfAppFireWall_UserVersion.MVVM.viewModel
{
    public class BooleanToStatusConverter : MarkupExtension, IValueConverter
    {
        private static BooleanToStatusConverter _converter;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new BooleanToStatusConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool allowed = (bool)value;
                return allowed ? "Allowed" : "Blocked";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
