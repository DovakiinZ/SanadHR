import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function PaymentMethodsPage() {
  return (
    <SimpleMasterDataList
      objectType="PaymentMethod"
      title="طرق الدفع"
      description="تحويل بنكي، نقدي، بطاقة راتب — تؤثر على تصدير الرواتب"
      backHref="/settings/payroll"
      backLabel="إعدادات الرواتب"
    />
  );
}
