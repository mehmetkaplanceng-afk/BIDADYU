import React, { useState } from 'react';
import { Card, Form, Input, InputNumber, Button, message, Alert, Space } from 'antd';
import { ClusterOutlined } from '@ant-design/icons';
import api from '../services/api';

export const VirtualLabPage: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const [form] = Form.useForm();

  const handleGenerate = async (values: any) => {
    setLoading(true);
    try {
      const res = await api.post('/VirtualLab/generate-lab', values);
      message.success(res.data.message);
      form.resetFields();
    } catch (err: any) {
      message.error('Sanal Laboratuvar oluşturulamadı.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: 700, margin: '0 auto' }}>
      <Card title={<Space><ClusterOutlined style={{ color: '#1677ff' }} /><span>Sanal Bilgisayar & Laboratuvar Oluşturucu</span></Space>}>
        <Alert
          message="Sanal İstemci Simülasyonu"
          description="Buradan tek tıkla 10, 25, 50 bilgisayarlık Sanal Bilgisayar Laboratuvarları üretebilir, bu bilgisayarları hemen Cihazlar listesine ekleyerek buluttan yazılım yükleme testleri yapabilirsiniz."
          type="info"
          showIcon
          style={{ marginBottom: 24 }}
        />
        <Form form={form} layout="vertical" onFinish={handleGenerate} initialValues={{ prefix: 'PC-LAB', computerCount: 15, building: 'Mühendislik Fakültesi', floor: 'Kat 2' }}>
          <Form.Item name="labName" label="Laboratuvar / Grup Adı" rules={[{ required: true, message: 'Lab adı giriniz!' }]}>
            <Input placeholder="Örn: Bilgisayar Laboratuvarı 1" />
          </Form.Item>
          <Form.Item name="prefix" label="Bilgisayar İsim Öneki" rules={[{ required: true }]}>
            <Input placeholder="Örn: PC-LAB1" />
          </Form.Item>
          <Form.Item name="computerCount" label="Oluşturulacak Sanal Cihaz Sayısı" rules={[{ required: true }]}>
            <InputNumber min={1} max={100} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="building" label="Bina / Blok">
            <Input />
          </Form.Item>
          <Form.Item name="floor" label="Kat / Konum">
            <Input />
          </Form.Item>
          <Button type="primary" htmlType="submit" loading={loading} block size="large">
            Sanal Laboratuvarı ve Bilgisayarları Üret
          </Button>
        </Form>
      </Card>
    </div>
  );
};
